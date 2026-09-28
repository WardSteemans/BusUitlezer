using System.IO.Ports;
using TransitObd.Protocol;

if (args.Length == 0)
{
    Console.WriteLine("Geen COM-poort opgegeven. Beschikbare poorten:");
    var ports = SerialPort.GetPortNames();
    if (ports.Length == 0)
    {
        Console.WriteLine("  (geen gevonden — is de adapter aangesloten?)");
    }
    foreach (var p in ports)
    {
        Console.WriteLine($"  {p}");
    }
    Console.WriteLine();
    Console.WriteLine("Gebruik: dotnet run -- demo               (emulator, geen adapter nodig)");
    Console.WriteLine("     of: dotnet run -- <COM-poort> [baudrate]");
    return;
}

if (string.Equals(args[0], "demo", StringComparison.OrdinalIgnoreCase))
{
    await RunDemoAsync();
    return;
}

var portName = args[0];
var baudRate = args.Length > 1 ? int.Parse(args[1]) : 38400;

using var port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
{
    ReadTimeout = 5000,
    WriteTimeout = 2000,
    NewLine = "\r",
};

Console.WriteLine($"Verbinden met {portName} @ {baudRate} baud...");
port.Open();

SendCommand(port, "ATZ");   // reset adapter
SendCommand(port, "ATE0");  // command echo uit
SendCommand(port, "ATI");   // adapter identificatie

static void SendCommand(SerialPort port, string command)
{
    Console.WriteLine($"> {command}");
    port.Write(command + "\r");

    try
    {
        var response = port.ReadTo(">");
        Console.WriteLine($"< {response.Trim()}");
    }
    catch (TimeoutException)
    {
        Console.WriteLine("< (timeout, geen antwoord)");
    }
}

static async Task RunDemoAsync()
{
    Console.WriteLine("Demo-modus: J1979Client tegen FakeObdLink (emulator, geen adapter aangesloten).");
    Console.WriteLine();

    var client = new J1979Client(new FakeObdLink());

    Pid[] pids =
    [
        Pid.EngineRpm,
        Pid.VehicleSpeed,
        Pid.CoolantTemp,
        Pid.IntakeAirTemp,
        Pid.ThrottlePosition,
        Pid.FuelTankLevel,
        Pid.ControlModuleVoltage,
    ];

    var values = await client.ReadLiveDataAsync(pids);
    Console.WriteLine("Live data:");
    foreach (var value in values)
    {
        Console.WriteLine($"  {value.Pid,-20} {value.InterpretedValue,10:0.###} {value.Unit}");
    }

    Console.WriteLine();
    var vin = await client.ReadVinAsync();
    Console.WriteLine($"VIN: {vin} (emulator-testwaarde, geen echte Transit-VIN)");

    Console.WriteLine();
    var dtcs = await client.ReadStoredDtcsAsync();
    Console.WriteLine($"Opgeslagen DTC's ({dtcs.Count}):");
    foreach (var dtc in dtcs)
    {
        Console.WriteLine($"  {dtc.Code} — {dtc.Description}");
    }
}
