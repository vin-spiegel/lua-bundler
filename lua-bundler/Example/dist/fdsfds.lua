-- Bundled Files: 3
-- Unused Files: 1
-- Bundled At: 09/13/2022 08:09:42
local __modules = {}
local require = function(path)
    local module = __modules[path]
    if module ~= nil then
        if not module.inited then
            module.cached = module.loader()
            module.inited = true
        end
        return module.cached
    else
        error('module not found')
        return nil
    end
end
----------------
__modules["main"] = { inited = false, cached = false, loader = function(...)
---- START main.lua ----
	require("lib/logger")	local Test = require("test")	print("Logger Test")	Test.Run()
---- END main.lua ----
end }
----------------
__modules["lib/logger"] = { inited = false, cached = false, loader = function(...)
---- START lib/logger.lua ----
	local Logger = {}	function Logger.Log(msg)	    print("Logger.Log" .. msg)	end	print("module Logger loaded")	return Logger
---- END lib/logger.lua ----
end }
----------------
__modules["test"] = { inited = false, cached = false, loader = function(...)
---- START test.lua ----
	local Logger = require("lib/logger")	local Test = {}	function Test.Run()	    Logger.Log("Test.Run")	end	print("module Test loaded")	return Test
---- END test.lua ----
end }
-- Execute Main Function
__modules["main"].loader()