using System.IO.Ports;

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
    Console.WriteLine("Gebruik: dotnet run -- <COM-poort> [baudrate]");
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
