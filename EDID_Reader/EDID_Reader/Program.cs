using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;


namespace EDID_Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("EDID dump for connected monitors.\n");
            Console.Out.WriteLine("Please contact 'zackwang0821@gmail.com' if any question.");


            var mc = new System.Management.ManagementClass(string.Format(@"\\{0}\root\wmi:WmiMonitorDescriptorMethods", Environment.MachineName));

            foreach (ManagementObject mo in mc.GetInstances()) //Do this for each connected monitor
            {
                Console.Out.WriteLine("\n");
                Console.Out.WriteLine($"Monitor: {mo.GetPropertyValue("InstanceName")}");

                for (int i = 0; i < 256; i++)
                {
                    var inParams = mo.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                    inParams["BlockId"] = i;

                    ManagementBaseObject outParams = null;
                    try
                    {
                        outParams = mo.InvokeMethod("WmiGetMonitorRawEEdidV1Block", inParams, null);
                        uint blktype = Convert.ToUInt16(outParams["BlockType"]);
                        Console.Out.WriteLine("Block[{2}] type {0}, content of type {1} ",
                                          blktype, outParams["BlockContent"].GetType(), i);
                        // Types are: 0-invalid 1=base block 2=block map 255=other (ext.block)
                        if ((blktype == 1) || (blktype == 255))
                        {
                            Byte[] a = (Byte[])outParams["BlockContent"];
                            Console.Out.WriteLine($"# Length={a.Length} Cksm 0x{a[127].ToString("X2")}");

                            if ((blktype == 1) && (a.Length == 128) && (a[126] != 0))
                            {
                                Console.Out.WriteLine($"# Main block. Number of extensions: {a[126]}");
                            }

                            for (int k = 0; k < a.Length; k++)
                            {
                                Console.Out.Write("{0:X2} ", a[k]);
                                if ((k % 16) == 15)
                                    Console.Out.WriteLine();
                            }
                        }
                        else
                        {
                            Console.Out.WriteLine("# weird block [{0}] type", i);
                        }
                    }
                    catch { break; } //No more EDID blocks

                }
            }
            Console.ReadLine();
        }

    }
}
