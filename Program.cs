using System;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommandLine;

namespace sping {
    class Program {

        private static bool ValidateIPv4String(string ipString) {
            if (String.IsNullOrWhiteSpace(ipString)) {
                return false;
            }
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4) {
                return false;
            }    
            byte tempForParsing;
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private static Random random = new Random();
        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static PingReply PingHost(string host, int timeout, int payloadSize, int timeToLive, Boolean dontFragment) {
            Ping pinger = null;
            byte[] payload = Encoding.ASCII.GetBytes(RandomString(payloadSize));
            PingOptions options = new PingOptions (timeToLive, dontFragment);
            PingReply reply = null;

            try {
                pinger = new Ping();
                reply = pinger.Send(host, timeout, payload, options);
            } catch (PingException pe) {
                throw pe;
            } finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }    
            return reply;
        }

        class Options {
                    [Value(0, MetaName = "address", Required = true, HelpText = "The hostname or IP address.")]
                    public string Address { get; set; }

                    [Option('c', "count", Default = 1, Required = false, HelpText = "The total number of echo requests to make.")]
                    public int Count {get; set; }

                    [Option('p', "payload", Default = 32, Required = false, HelpText = "Byte payload of packet.")]
                    public int Payload {get; set; }

                    [Option('t', "timeout", Default = 1000, Required = false, HelpText = "Maximum time (in milliseconds) to wait for the reply.")]
                    public int Timeout {get; set; }

                    [Option('T', "timeToLive", Default = 128, Required = false, HelpText = "Maximum number of hops before packet should be discarded.")]
                    public int TimeToLive {get; set; }

                    [Option('f', "dontFragment", Default = false, Required = false, HelpText = "Whether the packet is allowed to be fragmented.")]
                    public bool DontFragment {get; set; }

                    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
                    public bool Verbose { get; set; }
                }

        private static void HandleParseError(IEnumerable<Error> errs) {
            if (errs.IsVersion()) {
                Console.WriteLine("Version Request");
                return;
            }
            if (errs.IsHelp()) {
                Console.WriteLine("Help Request");
                return;
            }
            // Console.WriteLine("Parser Fail");
            System.Environment.Exit(1);
        }

        static void Main(string[] args) {   

            string address = "";
            int count = 1;
            int payload = 32;
            int timeout = 3333;
            int timeToLive = 128;
            Boolean dontFragment = false;

            var pargs = Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
                address = o.Address;
                count = o.Count;
                payload = o.Payload;
                timeout = o.Timeout;
                timeToLive = o.TimeToLive;
                dontFragment = o.DontFragment;
            }).WithNotParsed(HandleParseError);

            Console.WriteLine("Sending " + count + " pings to " + address + " with payload: " + payload + "...");
            // need to check input for valid DNS/IP
            if (!ValidateIPv4String(address)) {
                // see if it's a valid hostname
                if (Uri.CheckHostName(address) == 0) {
                    Console.WriteLine("address does not appear to be a valid hostname or IP address!");
                    System.Environment.Exit(1);
                }
            }

            ConcurrentBag<PingReply> replies = new ConcurrentBag<PingReply>();
            Parallel.For(0, count, index => { 
                try {
                    replies.Add(PingHost(address, timeout, payload, timeToLive, dontFragment));
                } catch (PingException pe) {
                    Console.WriteLine(pe.InnerException);
                }
            });

            int replyCount = 0;
            // List<long> rtts = new List<long>();
            long rtts = 0;
            foreach (var reply in replies) {
                replyCount++;
                // rtts.Add(reply.RoundtripTime);
                rtts += reply.RoundtripTime;
            }
            int ratioSize = (int) (Math.Floor(Math.Log10(count) + 1) * 2) + 1;
            string ratio = replyCount + "/" + count;
            // double avgRtt = rtts.Average();
            long avgRtt = rtts / replyCount;
            Console.WriteLine("Name".PadRight(12) + "  " + "Ratio".PadLeft(ratioSize) + "  " + "PktLoss" + "  " + "AvgRTT".PadLeft(9));
            Console.WriteLine(address.PadRight(12) + "  " + ratio.PadLeft(ratioSize) + "  " + (1 - replyCount / count).ToString("F2").PadLeft(7) + "  " + avgRtt.ToString("F4").PadLeft(9));
        }
    }
}
