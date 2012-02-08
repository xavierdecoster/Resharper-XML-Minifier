namespace Resharper.Plugins.Minify
{
    public interface IMinifier
    {
        string Minify(string input);
    }
}