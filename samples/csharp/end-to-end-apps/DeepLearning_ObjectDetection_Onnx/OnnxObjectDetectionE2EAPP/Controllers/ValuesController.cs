using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnnxObjectDetectionE2EAPP.OnnxModelScorers;

namespace TensorFlowImageClassificationWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        static string imagesFolderRelativePath = @"ImagesList";
        static string imagesFolderPath = ModelHelpers.GetAbsolutePath(imagesFolderRelativePath);
        public class Result
        {
            public string encodedImageString { get; set; }
            public string imageFileName { get; set; }
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            
            string supportedExtensions = "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,";
            var filePaths = Directory.GetFiles(imagesFolderPath, "*.*", SearchOption.AllDirectories).
                Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower()));
            List<Result> resultImages = new List<Result>();
            Result result = null;

            foreach(var imageFilePath in filePaths)
            {
                Image img = Image.FromFile(imageFilePath);
                var imageName = Path.GetFileName(imageFilePath);
                using (MemoryStream m = new MemoryStream())
                {
                    img.Save(m, img.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    result = new Result { encodedImageString = base64String , imageFileName = imageName };
                }
                resultImages.Add(result);
            }
            return Ok(resultImages);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string fileName)
        {
            DirectoryInfo dir = new DirectoryInfo(imagesFolderPath);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Equals(fileName))
                {
                    string imageFilePath = Path.Combine(imagesFolderPath, fileName);                    
                }
                    
            }
            var x = fileName;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
