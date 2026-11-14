using System.IO.Pipes;
using System.Text.Json;

namespace JVRSMPLAY.Desktop;

/// <summary>
/// Receives newline-delimited JSON from the local ETS2 telemetry plugin.
/// Protocol v1: consumption messages include an available flag; valid values use L/100km.
/// The pipe is local-only; no TCP listener is opened.
/// </summary>
internal sealed class TelemetryPipeClient
{
    public const string PipeName = "JVRSMPLAY.Telemetry.v1";

    public async Task RunAsync(
        Action<double> onConsumption,
        Action onConsumptionUnavailable,
        Action<bool> onConnectionChanged,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var connected = false;
            try
            {
                using var pipe = new NamedPipeClientStream(
                    ".", PipeName, PipeDirection.In, PipeOptions.Asynchronous);

                await pipe.ConnectAsync(1000, cancellationToken);
                using var reader = new StreamReader(pipe);

                connected = true;
                onConnectionChanged(true);

                while (pipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line is null) break;

                    try
                    {
                        using var document = JsonDocument.Parse(line);
                        var root = document.RootElement;

                        if (!root.TryGetProperty("type", out var type) ||
                            type.ValueKind != JsonValueKind.String ||
                            type.GetString() != "consumption" ||
                            !root.TryGetProperty("available", out var availableElement) ||
                            (availableElement.ValueKind != JsonValueKind.True &&
                             availableElement.ValueKind != JsonValueKind.False))
                        {
                            continue;
                        }

                        if (availableElement.ValueKind == JsonValueKind.False)
                        {
                            onConsumptionUnavailable();
                            continue;
                        }

                        if (!root.TryGetProperty("value", out var valueElement) ||
                            !valueElement.TryGetDouble(out var value) ||
                            !double.IsFinite(value) ||
                            value < 0 ||
                            !root.TryGetProperty("unit", out var unitElement) ||
                            unitElement.ValueKind != JsonValueKind.String ||
                            unitElement.GetString() != "L/100km")
                        {
                            continue;
                        }

                        onConsumption(value);
                    }
                    catch (JsonException)
                    {
                        // Ignore malformed messages; never display unvalidated data.
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (TimeoutException)
            {
                // The game/plugin is not running yet. Retry quietly.
            }
            catch (IOException)
            {
                // The game closed or the plugin disconnected. Retry quietly.
            }
            finally
            {
                if (connected) onConnectionChanged(false);
            }

            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
