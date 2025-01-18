using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FileServer.Controllers
{
    public class UploadImage
    {
        /// <summary>
        /// Це буде зображення у форматі base64
        /// </summary>
        public string Photo { get; set; } = String.Empty;
    }
    [Route("api/[controller]")]
    [ApiController]
    public class GalleriesController : ControllerBase
    {
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadImageAsync([FromBody] UploadImage model)
        {
            try
            {
                string fileName = $"{Guid.NewGuid()}.jpg";
                if (model.Photo.Contains(','))
                    model.Photo = model.Photo.Split(',')[1];
                byte[] byteArray = Convert.FromBase64String(model.Photo);

                using Image image = Image.Load(byteArray);

                image.Mutate(x => x.Resize(
                    new ResizeOptions
                    {
                        Size = new Size(600, 600),
                        Mode = ResizeMode.Max
                    }
                ));

                string folderName = "uploads";
                string pathFolder = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                string outputFilePath = Path.Combine(pathFolder, fileName);

                image.Save(outputFilePath);
                return Ok(new { image = $"/images/{fileName}" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
