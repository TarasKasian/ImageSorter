using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ImageSorter
{
    internal enum ResultColor 
    {
        Red,
        Green,
        Blue
    }

    // TODO: Cover another image formats;
    //       Improve color detection method.

    class Program
    {
        private static string _rootDirPath;

        private static string _redDir;

        private static string _greenDir;

        private static string _blueDir;

        private static string _searchPattern = @"*.jpg";

        private static string[] _filesPaths;

        private static Dictionary<string, Bitmap> _fileNameBitmapPairDict = new Dictionary<string, Bitmap>();

        //   key - old file name 
        // value - new file name
        private static ConcurrentDictionary<string, string> _fileNamesDict = new ConcurrentDictionary<string, string>();

        private static StaticticsInfo _statisticsInfo = new StaticticsInfo();

        private static Logger _logger = new Logger();

        static void Main(string[] args)
        {
            _rootDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            _statisticsInfo.StatisticsChanged += _logger.TraceLog;
            _statisticsInfo.OperationCompleted += _logger.TraceLog;

            CreateFolders();

            _filesPaths = Directory.GetFiles(_rootDirPath, _searchPattern, SearchOption.AllDirectories);

            _statisticsInfo.FilesGeneral = _filesPaths.Length;

            foreach (string fp in _filesPaths) 
            {
                _fileNameBitmapPairDict.Add(fp, GetBitmap(fp));
            }

            FillDictionary(_fileNameBitmapPairDict);

            MoveImages();

            Console.ReadKey();
        }

        private static void MoveImages() 
        {
            foreach (KeyValuePair<string, string> pair in _fileNamesDict)
            {
                File.Move(pair.Key, pair.Value);
            }
        }

        private static Bitmap GetBitmap(string fileName) 
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            Bitmap img = new Bitmap(fs);
            fs.Close();
            return img;
        }

        private static void CreateFolders() 
        {
            _redDir = _rootDirPath + @"\B";
            if (!Directory.Exists(_redDir)) 
                Directory.CreateDirectory(_redDir);

            _greenDir = _rootDirPath + @"\G";
            if (!Directory.Exists(_greenDir))
                Directory.CreateDirectory(_greenDir);

            _blueDir = _rootDirPath + @"\R";
            if (!Directory.Exists(_blueDir))
                Directory.CreateDirectory(_blueDir);
        }

        private static void FillDictionary(Dictionary<string, Bitmap> fileNameBitmapPairs) 
        {
            List<Task> tasks = new List<Task>();

            foreach (var pair in fileNameBitmapPairs) 
            {
                tasks.Add(Task.Factory.StartNew(() => DetermineNewImagePath(pair.Key, GetResultColorUnsafe(pair.Value) /* GetResultColor(pair.Value) */)));
            }

            Task.WhenAll(tasks).Wait();

            _statisticsInfo.IsOperationCompleted = true;
        }

        private static Task DetermineNewImagePath(string fileName, ResultColor resultColor) 
        {
            switch (resultColor)
            {
                case ResultColor.Red:
                    _fileNamesDict.TryAdd(fileName, _redDir + "\\" + Path.GetFileName(fileName));
                    break;
                case ResultColor.Green:
                    _fileNamesDict.TryAdd(fileName, _greenDir + "\\" + Path.GetFileName(fileName));
                    break;
                case ResultColor.Blue:
                    _fileNamesDict.TryAdd(fileName, _blueDir + "\\" + Path.GetFileName(fileName));
                    break;
            }

            _statisticsInfo.FilesProcessed++;

            return Task.CompletedTask;
        }

        private static ResultColor GetResultColorUnsafe(Bitmap image) 
        {
            BitmapData bData = 
                image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bData.Stride;

            IntPtr scan0 = bData.Scan0;

            int r = 0;
            int g = 0;
            int b = 0;

            unsafe 
            {
                byte* p = (byte*)(void*)scan0;

                int nOffset = stride - image.Width * 3;

                for (int i = 0; i < image.Height; ++i)
                {
                    for (int j = 0; j < image.Width; ++j)
                    {
                        byte maxVal = Math.Max(p[0], Math.Max(p[1], p[2]));

                        if (maxVal == p[0]) r++;
                        if (maxVal == p[1]) g++;
                        if (maxVal == p[2]) b++;

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            int maxColorSum = Math.Max(r, Math.Max(g, b));

            if (maxColorSum == r)
                return ResultColor.Red;

            if (maxColorSum == g)
                return ResultColor.Green;

            return ResultColor.Blue;
        }
    }
}
