/*
 * Copyright © VIVOXIE S DE RL DE CV
 *
 * @author Humberto Alonso Villegas<humberto@vivoxie.com>
 * @version 1.0
 */
using System;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
//using UnityEngine;
//using System.Collections;

public class pv1 //: MonoBehaviour
{
    /**
     * Instance to PowerClaw
     *
     * @name _serialPort
     * @type SerialPort
     * @access private
     */
    private static SerialPort _serialPort;

    /**
     * Determine if serial port is closed or open
     */
    private static bool _continue = false;

    /**
     * Read message PowerClaw
     *
     * @name _readThread
     * @type Thread
     * @access private
     */
    private static Thread _readThread;

    /**
     * Runtime sensation
     *
     * @name _timeThread
     * @type int
     * @access private
     */
    private static int _timeThread;

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update() { }

    /**
     * Create instance to PowerClaw
     * 
     * @method bool serialPortSet()
     * @access public
     * @param string com
     * @return bool
     */
    public bool serialPortSet(string com = "")
    {
        try
        {
            if (com == "")
            {
                com = serialPortGet();
            }
            _readThread = new Thread(Read);
            _serialPort = new SerialPort();
            _serialPort.PortName = com;
            _serialPort.BaudRate = 115200;
            _serialPort.DataBits = 8;
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), "none", true);
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "1", true);
            _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "none", true);
            return true;
        }
        catch (IOException e)
        {
            return false;
        }
    }

    /**
     * Open communication with PowerClaw
     * 
     * @method bool serialPortOpen()
     * @access public
     * @return bool
     */
    public bool serialPortOpen()
    {
        try
        {
            _serialPort.Open();
            _continue = true;
            _readThread.Start();
            //_serialPort.WriteLine("1");
            return true;
        }
        catch (IOException e)
        {
            return false;
        }
    }

    /**
     * Close communication with PowerClaw
     * 
     * @method bool serialPortOpen()
     * @access public
     * @return bool
     */
    public bool serialPortClose()
    {
        try
        {
            sensationSend("zero,thumb,0,vibration,100");
            _continue = false;
            _readThread.Abort();
            _serialPort.Close();
            return true;
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    /**
     * Return list of the available PowerClaw
     * 
     * @method string serialPortGet()
     * @access public
     * @return string
     */
    public string serialPortGet()
    {
        System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c REG QUERY HKEY_LOCAL_MACHINE\\HARDWARE\\DEVICEMAP\\SERIALCOMM");
        procStartInfo.RedirectStandardOutput = true;
        procStartInfo.UseShellExecute = false;
        procStartInfo.CreateNoWindow = false;
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo = procStartInfo;
        proc.Start();
        string[] split = Regex.Split(proc.StandardOutput.ReadToEnd(), Environment.NewLine);
        int i = 0;
        do
        {
            if (split[i].Contains(" COM"))
            {
                string[] spl = Regex.Split(System.Text.RegularExpressions.Regex.Replace(split[i].Trim(), @"\s{2,}", " "), " ");
                return spl[2];
            }
            i++;
        } while (i < split.Length);
        return "COM1";
    }

    /**
     * Send sensation to PowerClaw
     * 
     * @method string sensationSend()
     * @access public
     * @return string
     */
    private static string sensationSend(string sensation)
    {
        string list = "";
        int intensity, j;
        string[] split = sensation.Split(",".ToCharArray());
        if(!_continue)
            return "serial port is closed";
        switch (split[0] as string)
        {
            case "right":
                list += "r";
                break;
            case "left":
                list += "l";
                break;
            case "hands":
                list += "h";
                break;
            case "zero":
                list += "z";
                break;
            default:
                return "Invalid Hand";
        }

        switch (split[1] as string)
        {
            case "thumb":
                list += "t";
                break;
            case "index":
                list += "i";
                break;
            case "middle":
                list += "m";
                break;
            case "ring":
                list += "r";
                break;
            case "pinkie":
                list += "p";
                break;
            case "zero":
                list += "z";
                break;
            case "hand":
                list += "h";
                break;
            default:
                return "Invalid Finger";
        }

        if (!Int32.TryParse(split[2], out j))
            return "Invalid Phalange";
        list += split[2];

        switch (split[3] as string)
        {
            case "vibration":
                list += "v";
                break;
            case "roughness":
                list += "r";
                break;
            case "contact":
                list += "c";
                break;
            case "heat":
                list += "h";
                break;
            case "cold":
                list += "o";
                break;
            case "zero":
                list += "z";
                break;
            default:
                return "Invalid Sensation";
        }

        if (!Int32.TryParse(split[4], out intensity))
            return "Invalid Phalange";
        if (intensity < 0)
            split[4] = "000";
        else if (intensity > 100)
            split[4] = "100";
        else if (intensity < 10)
            split[4] = "00" + split[4];
        else if (intensity < 100)
            split[4] = "0" + split[4];
        list += split[4];
        list = "." + list;
        _serialPort.WriteLine(list);
        list = "";
        split = sensation.Split(",".ToCharArray());
        list = split[0] + "," + split[1] + "," + split[2] + "," + split[3] + ",000";
        if(_timeThread == -1 || split[0] == "zero" || split[1] == "zero" || split[3] == "zero" || intensity == 0)
            return "true";
        Thread.Sleep(_timeThread);
        Console.WriteLine(list);
        _timeThread = -1;
        sensationSend(list);
        return "true";
    }

    /**
     * Send sensation to Power Claw with runtime
     * 
     * @method string sensationSend()
     * @access public
     * @return string
     */
    public string sensationSend(string sensation, int time)
    {
        if (!_continue)
            return "serial port is closed";
        _timeThread = time;
        Thread thread = new Thread(() => sensationSend(sensation));
        thread.Start();
        return "true";
    }

    /**
     * Standalone
     *
    public static void Main()
    {
        pv1 SDK = new pv1();
        string message;
        _continue = true;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        SDK.serialPortSet();
        SDK.serialPortOpen();
        while (_continue)
        {
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
            SDK.sensationSend("zero,thumb,0,vibration,100", 0);
            message = Console.ReadLine();
            message = Console.ReadLine();
            SDK.sensationSend("right,ring,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,middle,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,index,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,thumb,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,pinkie,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,ring,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,middle,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,index,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,thumb,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("right,pinkie,0,vibration,1", 500000);


            SDK.sensationSend("zero,thumb,0,vibration,100", 0);


            SDK.sensationSend("left,ring,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,middle,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,index,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,thumb,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,pinkie,0,heat,100", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,ring,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,middle,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,index,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,thumb,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("left,pinkie,0,vibration,1", 500000);
            message = Console.ReadLine();
            SDK.sensationSend("zero,thumb,0,vibration,100", 0);
            /*SDK.sensationSend("left,ring,0,heat,100", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("left,middle,0,heat,100", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("left,index,0,heat,100", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("left,thumb,0,heat,100", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("left,pinkie,0,heat,100", 500000);
            Thread.Sleep(15);*



            SDK.sensationSend("right,ring,0,vibration,10", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("right,middle,0,vibration,10", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("right,index,0,vibration,10", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("right,thumb,0,vibration,10", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("right,pinkie,0,vibration,10", 500000);
            Thread.Sleep(15);
            SDK.sensationSend("left,ring,0,vibration,10", 1000);
            message = Console.ReadLine();
            SDK.sensationSend("left,middle,0,vibration,10", 1000);
            message = Console.ReadLine();
            SDK.sensationSend("left,index,0,vibration,10", 1000);
            message = Console.ReadLine();
            SDK.sensationSend("left,thumb,0,vibration,10", 1000);
            message = Console.ReadLine();
            SDK.sensationSend("left,pinkie,0,vibration,10", 1000);
            message = Console.ReadLine();
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
            /*SDK.sensationSend("zero,thumb,0,vibration,100", 0);
            SDK.sensationSend("right,thumb,0,roughness,90", 7000);
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
            //SDK.serialPortClose();
            Console.WriteLine(SDK.sensationSend("left,thumb,0,vibration,100", 1000));
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
            Console.WriteLine(SDK.sensationSend("left,thumb,0,heat,80", 1000));
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
            Console.WriteLine(SDK.sensationSend("left,thumb,0,cold,100", 2000));
            message = Console.ReadLine();
            if (stringComparer.Equals("quit", message))
                break;
        }
        SDK.serialPortClose();
        return;
    }*/

    /**
     * Read the result of sending data to PowerClaw
     * 
     * @method string Read()
     * @access private
     * @return void
     */
    private void Read()
    {
        while (_continue)
        {
            try
            {
                string message = _serialPort.ReadLine();
                Console.WriteLine(message);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
};
