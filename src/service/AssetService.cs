using System.IO;

namespace ChessBuddies
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