using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Search
{
    public class NoiseService : INoiseService
    {
        private IHostingEnvironment _hostingEnvironment;
        public NoiseService(
            IHostingEnvironment hostingEnvironment
            )
        {
            _hostingEnvironment = hostingEnvironment;

        }
        private List<string> _noise;

        public List<string> Noise()
        {
            if (_noise == null)
            {
                _noise = new List<string>();
                foreach (var line in System.IO.File.ReadLines(MapPath("app_data/noise.dat")))
                {
                    foreach (string word in line.Split(' '))
                    {
                        if (!_noise.Contains(word))
                        {
                            _noise.Add(word);
                        }
                    }
                }
            }

            return _noise;
        }

        private String MapPath(string path)
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, path);
        }
    }
}
