using System;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

namespace Main
{
    /// <summary>
    /// 鏈€灏忓彲鐢ㄧ殑 xLua 绠＄悊鍣細
    /// 1. 鍒濆鍖?LuaEnv
    /// 2. 娉ㄥ唽鏂囦欢鍔犺浇鍣?    /// 3. 鎵ц main.lua
    /// 4. 姣忓抚 Tick
    /// </summary>
    public class LuaManager : MonoBehaviour
    {
        public static LuaManager Instance { get; private set; }

        [Tooltip("鏄惁鍦ㄥ惎鍔ㄦ椂鑷姩鎵ц main.lua")]
        [SerializeField]
        private bool runMainOnAwake = true;

        [Tooltip("Lua 涓诲叆鍙ｆā鍧楀悕锛屼笉甯︽墿灞曞悕")]
        [SerializeField]
        private string entryModule = "main";

        [Tooltip("鏄惁鍦ㄥ垏鍦烘櫙鏃朵繚鐣?LuaManager")]
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private LuaEnv luaEnv;
        
        private Action<string> sayHello;

        public LuaEnv Env => luaEnv;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            Initialize();

            if (runMainOnAwake)
            {
                DoMain();
            }
            CallSayHello("FGNB");
        }

        public void Initialize()
        {
            if (luaEnv != null)
            {
                return;
            }

            luaEnv = new LuaEnv();
            luaEnv.AddLoader(CustomLoader);
        }

        public void DoMain()
        {
            if (luaEnv == null)
            {
                Initialize();
            }

            try
            {
                luaEnv.DoString($"require '{entryModule}'");
                sayHello = luaEnv.Global.Get<Action<string>>("say_hello");
                
            }
            catch (Exception e)
            {
                Debug.LogError($"[LuaManager] Failed to execute lua entry '{entryModule}'. {e}");
            }
        }

        public void SafeDoString(string scriptChunk, string chunkName = "chunk")
        {
            if (luaEnv == null)
            {
                Initialize();
            }

            try
            {
                luaEnv.DoString(scriptChunk, chunkName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LuaManager] Failed to execute lua chunk '{chunkName}'. {e}");
            }
        }

        public void CallSayHello(string msg)
        {
            if (sayHello == null)
            {
                Debug.LogWarning("[LuaManager] Lua function say_hello is not ready.");
                return;
            }

            sayHello(msg);
        }

        private void Update()
        {
            luaEnv?.Tick();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (luaEnv == null)
            {
                return;
            }

            luaEnv.Dispose();
            luaEnv = null;
        }

        private byte[] CustomLoader(ref string filePath)
        {
            var normalizedModuleName = filePath.Replace(".", "/");
            var candidatePaths = new[]
            {
                Path.Combine(Application.dataPath, "Lua", normalizedModuleName + ".lua"),
                Path.Combine(Application.streamingAssetsPath, "Lua", normalizedModuleName + ".lua"),
                Path.Combine(Application.streamingAssetsPath, "Lua", normalizedModuleName + ".lua.txt")
            };

            foreach (var candidatePath in candidatePaths)
            {
                if (!File.Exists(candidatePath))
                {
                    continue;
                }

                filePath = candidatePath;
                return Encoding.UTF8.GetBytes(File.ReadAllText(candidatePath));
            }

            throw new FileNotFoundException($"Lua file '{filePath}' was not found. Checked: {string.Join(", ", candidatePaths)}");
        }
    }
}

