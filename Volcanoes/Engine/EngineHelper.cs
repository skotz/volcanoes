using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Engine
{
    class EngineHelper
    {
        private Dictionary<string, Func<IEngine>> _engines;

        public EngineHelper()
        {
            _engines = new Dictionary<string, Func<IEngine>>();
        }

        public void Add<T>(string name) where T : IEngine
        {
            _engines.Add(name, () => (IEngine)Activator.CreateInstance(typeof(T)));
        }

        public void Add(string name, Func<IEngine> creator)
        {
            _engines.Add(name, creator);
        }

        public List<string> EngineNames
        {
            get
            {
                return _engines.Select(x => x.Key).ToList();
            }
        }
        
        public IEngine GetEngine(string name)
        {
            if (_engines.ContainsKey(name))
            {
                return _engines[name]();
            }

            return null;
        }
    }
}
