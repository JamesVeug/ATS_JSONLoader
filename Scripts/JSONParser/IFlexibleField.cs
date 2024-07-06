using ATS_API;
using JLPlugin;

namespace TinyJson
{
    public interface IFlexibleField
    {
        bool ContainsKey(string key);
        void SetValueWithKey(string key, string value);
        string ToJSON(string prefix);
    }

    public interface IInitializable
    {
        public void Initialize();
    }
}