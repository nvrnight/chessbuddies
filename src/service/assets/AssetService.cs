using System.IO;

namespace src
{
    public interface IAssetService
    {
        string GetImagePath(string name);
    }
    public class AssetService : IAssetService
    {
        public string GetImagePath(string name)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "assets", "images", name);
        }
    }
}