using System.Numerics;

namespace ReBuff.Config
{
    public interface IConfigPage
    {
        string Name { get; }

        IConfigPage GetDefault();
        void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY);
    }
}
