using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CANTestBed
{
    public class Program
    {
        public static void Main()
        {
            int cnt = 0;
            bool bootState = false;
            bool txLedState = false;
            bool rxLedState = false;
            
            OutputPort txled = new OutputPort(Pins.GPIO_PIN_D8, txLedState);
            OutputPort rxled = new OutputPort(Pins.GPIO_PIN_D7, rxLedState);
            OutputPort bootLed = new OutputPort(Pins.ONBOARD_LED, bootState);

            // Blink the netduino led to indicate boot state.
            for (int i = 0; i < 3; i++)
            {
                bootLed.Write(bootState);
                Thread.Sleep(500);
                bootState = !bootState;
            }

            // Create the CAN Handler.
            MCP2515 CANHandler = new MCP2515();
            CANHandler.InitCAN(MCP2515.enBaudRate.CAN_BAUD_500K);
            // Set to normal operation mode.
            CANHandler.SetCANNormalMode();

            // Create standard TX message.
            MCP2515.CANMSG txMessage = new MCP2515.CANMSG();
            txMessage.data = new byte[] { 0xCC, 0xAA, 0xAA, 0xAA, 0x11, 0x00, 0xFF, 0xFF };
            txMessage.CANID = 0x1AA;

            // Create extended TX message.
            MCP2515.CANMSG txMessageExt = new MCP2515.CANMSG();
            txMessageExt.CANID = 0x1FEDCBA1;
            txMessageExt.data = new byte[] { 0x00, 0xEE };

            // Create the message that will hold received messages.
            MCP2515.CANMSG rxMessage = new MCP2515.CANMSG();

            while (true)
            {
                // Put a counter in the message.
                txMessage.data[6] = (byte)(cnt>>8);
                txMessage.data[7] = (byte)cnt;
                // Transmit messages.
                CANHandler.Transmit(txMessage, 10);
                CANHandler.Transmit(txMessageExt, 10);
                // Blink CAN shield TX led every 5 cycles.
                txLedState = (cnt % 5 == 0) ? !txLedState : txLedState;
                txled.Write(txLedState);
                // Check if a message was received.
                if (CANHandler.Receive(out rxMessage, 20))
                {
                    rxled.Write(rxLedState);
                    if (rxMessage.IsExtended)
                    {
                        rxLedState = !rxLedState;
                    }
                    else
                    { 
                    }
                }
                // Increase counter.
                cnt++;
            }
        }

    }
}
