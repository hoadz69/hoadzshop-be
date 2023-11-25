using System;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Core.Utility
{
    public class NetworkUtility
    {
        public static string GetClientIp(HttpRequest httpRequest)
        {
            try
            {
                string ip = httpRequest?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                if (ip == "::1")
                {
                    var hostName = Dns.GetHostName();
                    var ipHostEntry = Dns.GetHostEntry(hostName);
                    // System.Net.Dns.GetHostEntry(hostnam) lấy tập hợp các IP của client bao gồm các IPv6 ở trên và các IPv4 ở dưới
                    //ip trên cùng là ip card net chính, các ip sau có thể là ip máy ảo
                    // Lấy ipv4 của card net phải lấy tổng số (IP\2)
                    if (ipHostEntry.AddressList.Length > 0)
                    {
                        ip = ipHostEntry
                            .AddressList[Convert.ToInt32(Math.Floor((double) (ipHostEntry.AddressList.Length) / 2))]
                            .ToString();
                    }
                }

                return ip;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}