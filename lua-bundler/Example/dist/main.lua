-- Bundled Files: 3
-- Unused Files: 1
-- Bundled At: 09/13/2022 08:26:10
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
	require("lib/logger")
---- END main.lua ----
end }
----------------
__modules["lib/logger"] = { inited = false, cached = false, loader = function(...)
---- START lib/logger.lua ----
	local Logger = {}
---- END lib/logger.lua ----
end }
----------------
__modules["test"] = { inited = false, cached = false, loader = function(...)
---- START test.lua ----
	local Logger = require("lib/logger")
---- END test.lua ----
end }
-- Execute Main Function
__modules["main"].loader()