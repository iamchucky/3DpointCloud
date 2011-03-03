using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Magic.Network;
using System.IO.MemoryMappedFiles;

namespace Playback
{
    public partial class frmMain : Form
    {
        private static Stream ind_stream = null; // index file stream
        private string indexFileName;

        private static int curPacketNum;
        private static double curLTS;
        private static UdpClient udp;

        private static double TIME_MULTIPLIER;

        private HardcodedAddressProvider addressProvider = new HardcodedAddressProvider();
        private int numPorts;
        volatile bool[] doTransmit;

        int keyPortVal = -1;

        public int nBytesReadFromHD = 0;

        IPAddress redirectDestAddr;

        static bool mutexCreated;
        Mutex mutex;
        MemoryMappedFile mmf;

        class PreCachedReader
        {
            private const int bufSize = 1024 * 1024 * 10;
            private Stream fs = null;

            private byte[] data;
            private long offset;
            private int begin, end;

            public int nBytesReadFromHD = 0;

            public PreCachedReader(Stream stream)
            {
                fs = stream;
                offset = stream.Position;
                data = new byte[bufSize];

                begin = 0;
                end = 0;
                nBytesReadFromHD += LoadNextChunk();
            }

            public void Close()
            {
                fs.Close();
                fs = null;
                begin = end = 0;
            }

            public void SeekAbsolute(long off)
            {
                if (off == offset) return;

                if (off > offset && (off - offset) < bufSize)
                {
                    begin = (begin + (int)(off - offset)) % bufSize;
                    offset = off;
                    UpdateBuffer();
                }
                else
                {
                    lock (this)
                    {
                        fs.Seek(off, SeekOrigin.Begin);
                        offset = off;
                        begin = 0;
                        end = 0;
                        nBytesReadFromHD += LoadNextChunk();
                    }
                }
            }

            private int LoadNextChunk()
            {
                if (fs == null) return 0;

                int toLoad, ret = 0;

                lock (this)
                {
                    if (begin > end)
                    {
                        ret = toLoad = begin - end - 1;
                        if (toLoad <= 0) return 0;
                        fs.Read(data, end, toLoad);
                        end = (end + toLoad) % bufSize;
                    }
                    else if (begin == 0)
                    {
                        ret = toLoad = bufSize - end - 1;
                        if (toLoad <= 0) return 0;
                        fs.Read(data, end, toLoad);
                        end = (end + toLoad) % bufSize;
                    }
                    else if (end == 0)
                    {
                        ret = toLoad = begin - 1;
                        if (toLoad <= 0) return 0;
                        fs.Read(data, 0, toLoad);
                        end = (end + toLoad) % bufSize;
                    }
                    else // begin is less than end and neither end nor begin are 0
                    {
                        ret = toLoad = bufSize - end;
                        fs.Read(data, end, toLoad);
                        end = (end + toLoad) % bufSize;
                        toLoad = begin - end - 1;
                        fs.Read(data, end, toLoad);
                        end = (end + toLoad) % bufSize;
                        ret += toLoad;
                    }

                }
                return ret;
            }

            public void Preload()
            {
                int dataCnt = (end - begin + bufSize) % bufSize;

                if ((bufSize - dataCnt) > 3 * 1024 * 1024)
                {
                    nBytesReadFromHD += LoadNextChunk();
                }
            }

            private void UpdateBuffer()
            {
                int dataCnt = (end - begin + bufSize) % bufSize;

                if (dataCnt < 1024 * 1024)
                {
                    nBytesReadFromHD += LoadNextChunk();
                }
            }

            public UInt32 ReadUInt32()
            {
                byte[] buf = new byte[sizeof(UInt32)];
                Read(buf, 0, sizeof(UInt32));
                return BitConverter.ToUInt32(buf, 0);
            }

            public Int32 ReadInt32()
            {
                byte[] buf = new byte[sizeof(Int32)];
                Read(buf, 0, sizeof(Int32));
                return BitConverter.ToInt32(buf, 0);
            }

            public UInt16 ReadUInt16()
            {
                byte[] buf = new byte[sizeof(UInt16)];
                Read(buf, 0, sizeof(UInt16));
                return BitConverter.ToUInt16(buf, 0);
            }

            public int Read(byte[] dest, int off, int cnt)
            {
                Array.Copy(data, begin, dest, 0, Math.Min(bufSize - begin, cnt));
                int remaining = cnt - (bufSize - begin);
                if (remaining > 0)
                {
                    Array.Copy(data, 0, dest, (bufSize - begin), remaining);
                }
                begin = (begin + cnt) % bufSize;
                offset += cnt;
                UpdateBuffer();
                return cnt;
            }
        };

        private static PreCachedReader reader = null;

        private int IndexToPort(int index)
        {
            if (index >= addressProvider.NetworkAddresses.Count) return 0;
            else return addressProvider.NetworkAddresses[index].Port;
        }

        Dictionary<int, int> portToIndex = null;
        private int PortToIndex(int port)
        {
            //lazy load
            if (portToIndex == null)
            {
                portToIndex = new Dictionary<int, int>(addressProvider.NetworkAddresses.Count);
                foreach (NetworkAddress na in addressProvider.NetworkAddresses)
                {
                    if (portToIndex.ContainsKey(na.Port) == false)
                        portToIndex.Add(na.Port, addressProvider.NetworkAddresses.IndexOf(na));
                }
            }

            if (portToIndex.ContainsKey(port) == false)
                return portToIndex.Count;
            else
                return portToIndex[port];

        }

        Dictionary<int, string> portToString = null;
        private string PortToString(int port)
        {

            //lazy load
            if (portToString == null)
            {
                portToString = new Dictionary<int, string>(addressProvider.NetworkAddresses.Count);
                foreach (NetworkAddress na in addressProvider.NetworkAddresses)
                {
                    if (portToString.ContainsKey(na.Port) == false)
                        portToString.Add(na.Port, na.Name);
                }
            }
            if (portToString.ContainsKey(port) == false)
                return "Unknown Port " + port.ToString();
            else
                return portToString[port];


        }

        private DataTable statsTable = new DataTable();
        DataView dv;

        struct PacketInfo
        {
            public long filePos;
            public double lts;
            public ushort port;
        }
        private static List<PacketInfo> frames = new List<PacketInfo>(100000);  // mapping from packet number to file positions

        public frmMain()
        {
            int test = Marshal.SizeOf(new PacketInfo());

            redirectDestAddr = IPAddress.Loopback;

            numPorts = PortToIndex(-123423) + 1;
            doTransmit = new bool[numPorts];

            InitializeComponent();

            udp = new UdpClient();
            //udp.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.31"), 0));
            tmrRun.Interval = 10;
            TIME_MULTIPLIER = 1;

            statsTable.Columns.Add("Name", typeof(string));
            statsTable.Columns.Add("Port", typeof(UInt16));
            statsTable.Columns.Add("Transmit", typeof(bool));
            statsTable.Columns.Add("NumPackets", typeof(int));

            dv = new DataView(statsTable);
            dv.RowFilter = "NumPackets > 0";

            statsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            statsGrid.DataSource = dv;

            statsGrid.Columns[0].ReadOnly = true;
            statsGrid.Columns[1].ReadOnly = true;
            statsGrid.Columns[2].ReadOnly = false;
            statsGrid.Columns[3].ReadOnly = true;
            statsGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            statsGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            statsGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            statsGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            if (Directory.Exists("c:\\Logs"))
                f.InitialDirectory = "C:\\Logs";
            else
                f.InitialDirectory = "Z:\\Logs";
            f.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
            f.FilterIndex = 1;
            f.RestoreDirectory = true;

            if (f.ShowDialog() == DialogResult.OK)
            {
                if (reader != null) reader.Close();
                reader = null;

                Stream fs = f.OpenFile();
                BinaryReader br = new BinaryReader(fs);

                indexFileName = f.FileName + ".ind";

                lblFileName.Text = f.FileName;

                Preprocess(fs, br);

                br = null;

                reader = new PreCachedReader(fs);
            }
        }

        private void ReadNextFrameSet()
        {
            if (!IsFrameNumValid(curPacketNum)) return;
            if (keyPortVal < 0) ReadNextFrame();
            else
            {
                int nRead = 0;
                while (nRead < 1) //limit the number of packets read this way..
                {
                    nRead++;
                    int read = ReadNextFrame();
                    if (read < 0 || read == keyPortVal) return;
                }
            }
            trackBar.Value = (int)curPacketNum;
        }

        private int ReadNextFrame()
        {
            if (!IsFrameNumValid(curPacketNum))
            {
                curPacketNum--;
                if (tmrRun.Enabled)
                {
                    btnStopGo.Image = Properties.Resources.Forward_or_Next_32_n_p;
                    tmrRun.Stop();
                }
                return -1;
            }

            //check the port is checked to send...
            if (doTransmit[PortToIndex(frames[curPacketNum].port)])
            {
                reader.SeekAbsolute(frames[curPacketNum].filePos);

                UInt32 ticks;
                double lts;
                Int32 src_ip, dest_ip;
                UInt16 port;
                UInt32 len;
                ticks = reader.ReadUInt32();
                lts = (double)ticks / 10000.0;
                src_ip = reader.ReadInt32();
                dest_ip = reader.ReadInt32();
                port = reader.ReadUInt16();
                len = reader.ReadUInt32();
                byte[] packet = new byte[len];
                reader.Read(packet, 0, (int)len);

                // SPECIAL CODE FOR CHUCK
                if (addressProvider.GetAddressByName("SharedMemoryCamera").Port == port)
                {
                    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                    {
                        BinaryWriter writer = new BinaryWriter(stream);
                        mutex.WaitOne();
                        writer.Write(packet);
                        mutex.ReleaseMutex();
                    }
                }

                else
                {
                    //

                    long dest_ip_long = ((long)dest_ip) & 0xFFFFFFFF;
                    IPEndPoint dest = new IPEndPoint(dest_ip_long, port);

                    if (chkRedirect.Checked)
                    {
                        dest.Address = redirectDestAddr;
                        //dest.Address = IPAddress.Parse("192.168.1.31");
                    }

                    udp.Send(packet, (int)len, dest);
                    //				Thread.SpinWait(100000);
                    Thread.Sleep(0);
                    if (port == 92)
                    {
                        BinaryReader br = new BinaryReader(new MemoryStream(packet, 2, (int)len - 2));
                        byte[] tmp = new byte[4];
                        br.Read(tmp, 0, 2);
                        uint ts_s = (uint)tmp[0] * 256 + (uint)tmp[1];
                        br.Read(tmp, 0, 4);
                        uint ts_t = (uint)tmp[0] * 16777216 + (uint)tmp[1] * 65536 + (uint)tmp[2] * 256 + (uint)tmp[3];
                        double vts = (double)ts_s + (double)ts_t / 10000.0;
                        lblVTS.Text = vts.ToString("F4") + " (" + curPacketNum.ToString() + ")" + " " + lts.ToString("F2");
                        txtGoToFrame.Tag = curPacketNum.ToString();
                    }
                    else if ((int)statsTable.Rows[PortToIndex(92)][3] == 0 || !doTransmit[PortToIndex(92)])
                    {
                        lblVTS.Text = "(" + curPacketNum.ToString() + ")" + " " + lts.ToString("F2");
                    }
                }
            }
            latestPacketLTS = frames[curPacketNum].lts;
            curLTS = frames[curPacketNum].lts;
            int ret = (int)frames[curPacketNum].port;
            curPacketNum++;
            return ret;
        }

        class KeyPortItem
        {
            private string name;
            private int port;
            public KeyPortItem(string name, int port)
            {
                this.name = name;
                this.port = port;
            }
            public override string ToString()
            {
                if (port > 0) return name + " (" + port.ToString() + ")";
                else return name;
            }
            public int Port
            {
                get { return port; }
            }
        };

        private void Preprocess(Stream fs, BinaryReader br)
        {
            frames.Clear();
            progressbar.Value = 0;
            progressbar.ForeColor = Color.MediumBlue;
            progressbar.Minimum = 0;

            statsTable.Clear();
            for (int i = 0; i < numPorts; i++)
            {
                int port = IndexToPort(i);
                object[] obj = new object[4];
                obj[0] = PortToString(port);
                obj[1] = port;
                obj[2] = true;
                obj[3] = 0;
                statsTable.Rows.Add(obj);
                doTransmit[i] = true;
            }

            fs.Seek(0, SeekOrigin.Begin);

            if (File.Exists(indexFileName))
            {
                ind_stream = new FileStream(indexFileName, FileMode.Open, FileAccess.Read);
                PreprocessFromInd();
            }
            else
            {
                PreprocessFromRawLog(fs, br);
            }

            progressbar.Value = progressbar.Maximum;
            progressbar.ForeColor = Color.Orange;
            lblFileName.BackColor = Color.PaleGreen;

            curPacketNum = 0;
            if (frames.Count == 0) curLTS = 0;
            else curLTS = frames[0].lts;
            fs.Seek(0, SeekOrigin.Begin);

            trackBar.Maximum = frames.Count;
            trackBar.LargeChange = frames.Count / 1000;
            trackBar.SmallChange = frames.Count / 10000;
            trackBar.TickFrequency = frames.Count / 10000;
            keyPort.Items.Clear();
            keyPort.Items.Add(new KeyPortItem("All", -1));
            for (int i = 0; i < numPorts; i++)
            {
                if ((int)statsTable.Rows[i][3] == 0) continue;
                int port = IndexToPort(i);
                keyPort.Items.Add(new KeyPortItem(PortToString(port), port));
            }
            keyPort.SelectedIndex = 0;
        }

        private void PreprocessFromInd()
        {
            ind_stream.Seek(0, SeekOrigin.Begin);
            BinaryReader ibr = new BinaryReader(ind_stream);

            progressbar.Maximum = (int)(ind_stream.Length);

            UInt32 ver = ibr.ReadUInt32();
            if (ver == 1)
            {
                PreprocessFromInd_v1(ibr);
                ibr.Close();
                ind_stream.Close();
                ind_stream = null;
                WriteIndFile();
            }
            else if (ver == 2)
            {
                PreprocessFromInd_v2(ibr);
                ind_stream.Close();
                ind_stream = null;
                WriteIndFile();
            }
            else if (ver == 3)
            {
                PreprocessFromInd_v3(ibr);
                ind_stream.Close();
                ind_stream = null;
            }
        }

        private void PreprocessFromInd_v1(BinaryReader ibr)
        {
            UInt32 numFrames = ibr.ReadUInt32();
            uint nRead = sizeof(UInt32) + sizeof(Int32);

            for (UInt32 frameNum = 0; frameNum < numFrames; frameNum++)
            {
                if ((frameNum % 1000) == 0)
                {
                    Application.DoEvents();
                    progressbar.Value = (int)(nRead);
                    double r = (double)progressbar.Value / (double)progressbar.Maximum * 255.0;
                    progressbar.ForeColor = Color.FromArgb(0, (int)r, 255 - (int)r);
                }

                PacketInfo v;
                v.filePos = ibr.ReadInt64();
                UInt32 ticks = ibr.ReadUInt32();
                v.lts = (double)ticks / 10000.0;
                v.port = ibr.ReadUInt16();
                frames.Add(v);

                nRead += sizeof(Int64) + sizeof(UInt32) + sizeof(UInt16);

                // update stats  
                int rowNum = PortToIndex(v.port);
                int i = (int)statsTable.Rows[rowNum][3];
                i++;
                statsTable.Rows[rowNum][3] = i;
            }
        }

        private void PreprocessFromInd_v2(BinaryReader ibr)
        {
            UInt32 numFrames = ibr.ReadUInt32();
            uint nRead = sizeof(UInt32) + sizeof(Int32);

            UInt16 nStatsEntries = ibr.ReadUInt16();
            nRead += sizeof(UInt16);

            for (int i = 0; i < nStatsEntries; i++)
            {
                Int32 port = ibr.ReadInt32();
                nRead += sizeof(Int32);
                Int32 cnt = ibr.ReadInt32();
                nRead += sizeof(Int32);

                // update stats  
                statsTable.Rows[PortToIndex(port)][3] = cnt;
            }

            for (UInt32 frameNum = 0; frameNum < numFrames; frameNum++)
            {
                if ((frameNum % 1000) == 0)
                {
                    Application.DoEvents();
                    progressbar.Value = (int)(nRead);
                    double r = (double)progressbar.Value / (double)progressbar.Maximum * 255.0;
                    progressbar.ForeColor = Color.FromArgb(0, (int)r, 255 - (int)r);
                }

                PacketInfo v;
                v.filePos = ibr.ReadInt64();
                UInt32 ticks = ibr.ReadUInt32();
                v.lts = (double)ticks / 10000.0;
                v.port = ibr.ReadUInt16();
                frames.Add(v);

                nRead += sizeof(Int64) + sizeof(UInt32) + sizeof(UInt16);
            }
        }

        private void PreprocessFromInd_v3(BinaryReader ibr)
        {
            UInt32 numFrames = ibr.ReadUInt32();
            uint nRead = sizeof(UInt32) + sizeof(Int32);

            UInt16 nStatsEntries = ibr.ReadUInt16();
            nRead += sizeof(UInt16);

            for (int i = 0; i < nStatsEntries; i++)
            {
                Int32 port = ibr.ReadInt32();
                nRead += sizeof(Int32);
                Int32 cnt = ibr.ReadInt32();
                nRead += sizeof(Int32);

                // update stats  
                statsTable.Rows[PortToIndex(port)][3] = cnt;
            }

            for (UInt32 frameNum = 0; frameNum < numFrames; frameNum++)
            {
                if ((frameNum % 1000) == 0)
                {
                    Application.DoEvents();
                    progressbar.Value = (int)(nRead);
                    double r = (double)progressbar.Value / (double)progressbar.Maximum * 255.0;
                    progressbar.ForeColor = Color.FromArgb(0, (int)r, 255 - (int)r);
                }

                PacketInfo v;
                v.filePos = ibr.ReadInt64();
                v.lts = ibr.ReadDouble();
                v.port = ibr.ReadUInt16();
                frames.Add(v);

                nRead += sizeof(Int64) + sizeof(UInt32) + sizeof(UInt16);
            }
        }

        private void PreprocessFromRawLog(Stream fs, BinaryReader br)
        {
            long fSize = fs.Length;
            long nRead = 0;

            progressbar.Maximum = (int)(fs.Length / 1024);

            int frameNum = 0;
            while (nRead < fSize)
            {
                if ((frameNum % 1000) == 0)
                {
                    Application.DoEvents();
                    progressbar.Value = (int)(nRead / 1024);
                    double r = (double)progressbar.Value / (double)progressbar.Maximum * 255.0;
                    progressbar.ForeColor = Color.FromArgb(0, (int)r, 255 - (int)r);
                }

                UInt32 ticks, src_ip, dest_ip;
                UInt16 port;
                UInt32 len;

                PacketInfo v;
                v.filePos = nRead;

                ticks = br.ReadUInt32();
                src_ip = br.ReadUInt32();
                dest_ip = br.ReadUInt32();
                port = br.ReadUInt16();
                len = br.ReadUInt32();
                br.ReadBytes((int)len);

                nBytesReadFromHD += sizeof(UInt32) * 4 + sizeof(UInt16) + (int)len;

                v.port = port;
                v.lts = (double)ticks / 10000.0;
                frames.Add(v);

                nRead += 18 + len;

                // update stats  
                int rowNum = PortToIndex(port);
                int i = (int)statsTable.Rows[rowNum][3];
                i++;
                statsTable.Rows[rowNum][3] = i;

                frameNum++;
            }

            WriteIndFile();
        }

        private void WriteIndFile()
        {
            FileStream ind_writer = new FileStream(indexFileName, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(ind_writer);
            UInt32 ver = 3;
            bw.Write(ver);
            bw.Write((UInt32)frames.Count);

            UInt16 nStatsEntries = (UInt16)statsTable.Rows.Count;
            bw.Write(nStatsEntries);

            for (int i = 0; i < nStatsEntries; i++)
            {
                Int32 port = (UInt16)statsTable.Rows[i][1];
                bw.Write(port);
                Int32 cnt = (Int32)statsTable.Rows[i][3];
                bw.Write(cnt);
            }

            foreach (PacketInfo pi in frames)
            {
                bw.Write(pi.filePos);
                bw.Write(pi.lts);
                bw.Write(pi.port);
            }
            bw.Close();
            ind_writer.Close();
        }

        private void btnStopGo_Click(object sender, EventArgs e)
        {
            if (chkBoxSharedMemory.Checked && mutexCreated == false && mmf == null)
            {
                mutex = new Mutex(true, "CamMutex", out mutexCreated);
                mmf = MemoryMappedFile.CreateNew("Global\\CamMappingObject", 307208);
            }
            if (tmrRun.Enabled)
            {
                btnStopGo.Image = Properties.Resources.Forward_or_Next_32_n_p;
                tmrRun.Stop();
            }
            else
            {
                btnStopGo.Image = Properties.Resources.Pause_32_n_p8;
                tmrRun.Start();
            }
        }

        private bool IsFrameNumValid(int frameNum)
        {
            return !(frameNum < 0 || frameNum >= frames.Count);
        }

        DateTime lastTick = DateTime.Now;

        private void tmrRun_Tick(object sender, EventArgs e)
        {
            //Debug.Print("--- " + curLTS.ToString());
            double secsElapsed = (DateTime.Now - lastTick).TotalSeconds;
            lastTick = DateTime.Now;

            if (secsElapsed > .3) secsElapsed = .3;

            double oldLTS = curLTS;
            double readUntilLTS = curLTS + TIME_MULTIPLIER * secsElapsed;
            int cnt = 0;

            while (true)
            {
                if (!IsFrameNumValid(curPacketNum))
                {
                    if (loopCheck.Checked && frames.Count > 0)
                    {
                        trackBar.Value = 0;
                        curPacketNum = trackBar.Value;
                        curLTS = frames[curPacketNum].lts;
                        break;
                    }

                    else
                    {
                        btnStopGo.Image = Properties.Resources.Forward_or_Next_32_n_p;
                        tmrRun.Stop();
                        break;
                    }
                }

                PacketInfo pi = frames[curPacketNum];
                if (pi.lts > readUntilLTS) break;

                ReadNextFrame();

                cnt++;
                if (cnt > 600) break;
            }

            if (oldLTS == curLTS && tmrRun.Enabled)
                curLTS = readUntilLTS;

            //Debug.Print(cnt.ToString());
            trackBar.Value = (int)curPacketNum;
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {

            if (!IsFrameNumValid((int)trackBar.Value)) return;
            latestPacketLTS = -1;
            if (tmrRun.Enabled)
            {
                btnStopGo.Image = Properties.Resources.Forward_or_Next_32_n_p;
                tmrRun.Stop();
            }

            curPacketNum = (int)trackBar.Value;
            curLTS = frames[curPacketNum].lts;
        }

        private void btnStepFwd_Click(object sender, EventArgs e)
        {
            ReadNextFrameSet();
        }

        private void btnStepBack_Click(object sender, EventArgs e)
        {
            Seek(-2);
            ReadNextFrameSet();
        }

        private void trackBarSpeed_Scroll(object sender, EventArgs e)
        {
            UpdateRunSpeed();
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            if (splitContainerMain.Panel2Collapsed)
            {
                splitContainerMain.Panel2Collapsed = false;
                this.Width += 300;
            }
            else
            {
                this.Width -= splitContainerMain.Panel2.ClientRectangle.Width;
                splitContainerMain.Panel2Collapsed = true;
            }
        }

        private void Seek(int cnt)
        {
            if (cnt == 0) return;

            int dir = (cnt > 0) ? 1 : -1;
            while (IsFrameNumValid(curPacketNum))
            {
                curPacketNum = curPacketNum + dir;
                if (keyPortVal < 0 || frames[curPacketNum].port == keyPortVal)
                {
                    cnt -= dir;
                    if (cnt == 0) break;
                }
            }

            curPacketNum = Math.Min(Math.Max(curPacketNum, 0), frames.Count);
        }

        private void btnResendLast_Click(object sender, EventArgs e)
        {
            if (tmrLoop.Enabled)
            {
                btnResendLast.BackColor = Color.LightGray;
                tmrLoop.Enabled = false;
            }
            else
            {
                btnResendLast.BackColor = Color.Red;
                tmrLoop.Enabled = true;
            }
        }

        private void tmrLoop_Tick(object sender, EventArgs e)
        {
            Seek(-1);
            ReadNextFrameSet();
        }

        private void statsGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // for checkboxes, must update stuff here!
            if (statsGrid.IsCurrentCellDirty)
            {
                statsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void statsGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            statsTable.AcceptChanges();
            for (int i = 0; i < numPorts; i++)
            {
                doTransmit[i] = (bool)statsTable.Rows[i][2];
            }
        }

        private void keyPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            keyPortVal = ((KeyPortItem)(keyPort.SelectedItem)).Port;
        }

        DateTime prevStatsTime = DateTime.Now;
        double prevStatsLTS = -1;
        double latestPacketLTS = -1;

        private void tmrStats_Tick(object sender, EventArgs e)
        {
            double span = (DateTime.Now - prevStatsTime).TotalMilliseconds / 1000.0;
            prevStatsTime = DateTime.Now;

            if (reader == null)
            {
                double hddBandwidth_kBps = (double)(nBytesReadFromHD) / span / 1024.0;
                nBytesReadFromHD = 0;
                lblHDDBandwith.Text = hddBandwidth_kBps.ToString("F2") + " kB/s";
            }
            else
            {
                double hddBandwidth_kBps = (double)(reader.nBytesReadFromHD + nBytesReadFromHD) / span / 1024.0;
                reader.nBytesReadFromHD = 0;
                nBytesReadFromHD = 0;
                lblHDDBandwith.Text = hddBandwidth_kBps.ToString("F2") + " kB/s";
            }

            if (reader == null)
            {
                lblPlaybackSpeed.Text = "---";
            }
            else
            {
                if (prevStatsLTS > 0)
                {
                    double tmp = (latestPacketLTS - prevStatsLTS) / span;
                    lblPlaybackSpeed.Text = tmp.ToString("F2") + "s/s";
                }
            }
            prevStatsLTS = latestPacketLTS;
        }

        private void tmrPreload_Tick(object sender, EventArgs e)
        {
            if (reader != null)
                reader.Preload();
        }

        private void lblVTS_Click(object sender, EventArgs e)
        {
            if (txtGoToFrame.Tag == null) return;
            txtGoToFrame.Text = (string)txtGoToFrame.Tag;
        }

        private void btnGoToFrame_Click(object sender, EventArgs e)
        {
            int desiredFrame = 0;
            try
            {
                desiredFrame = Int32.Parse(txtGoToFrame.Text);
            }
            catch
            {
                MessageBox.Show("Invalid frame: " + txtGoToFrame.Text);
                return;
            }
            if ((desiredFrame > trackBar.Maximum) || (desiredFrame < trackBar.Minimum))
            {
                MessageBox.Show("Invalid frame: " + txtGoToFrame.Text);
                return;
            }
            trackBar.Value = desiredFrame;
            if (!IsFrameNumValid((int)trackBar.Value)) return;
            latestPacketLTS = -1;
            curPacketNum = desiredFrame;
            curLTS = frames[curPacketNum].lts;
            if (tmrRun.Enabled)
            {
                btnStopGo_Click(this, null);
            }
        }

        private void chkRedirect_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRedirect.Checked)
            {
                chkRedirect.BackColor = Color.PaleGreen;
                chkRedirect.Text = "Redirecting";
            }
            else
            {
                chkRedirect.BackColor = Color.Red;
                chkRedirect.Text = "Broadcasting";
            }
        }

        public void UpdateRunSpeed()
        {
            if (chkTurbo.Checked)
                TIME_MULTIPLIER = Math.Pow(2, ((trackBarSpeed.Value / 100.0) - 3) * 2);
            else
                TIME_MULTIPLIER = Math.Pow(2, (trackBarSpeed.Value / 100.0) - 3);
        }
        private void txtRedirect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            try
            {
                IPHostEntry iphe = System.Net.Dns.GetHostEntry(txtRedirect.Text);
                if (iphe == null) return;
                IPAddress[] addr = iphe.AddressList;
                for (int i = 0; i < addr.Length; i++)
                {
                    if (addr[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        redirectDestAddr = addr[0];
                        break;
                    }
                }
                txtRedirect.BackColor = Color.PaleGreen;
            }
            catch
            {
                txtRedirect.BackColor = Color.Pink;
            }
        }

        private void chkTurbo_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRunSpeed();
        }

    }
}