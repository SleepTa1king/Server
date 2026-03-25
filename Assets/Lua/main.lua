print("[Lua] main.lua loaded")

local CS = CS

CS.UnityEngine.Debug.Log("[Lua] Hello from main.lua")

function say_hello(name)
    CS.UnityEngine.Debug.Log("[Lua] Hello, " .. tostring(name))
end
