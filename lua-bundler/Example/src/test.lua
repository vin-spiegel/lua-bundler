local Logger = require("lib/logger")

local Test = {}

function Test.Run()
    Logger.Log("Test.Run")
end

print("module Test loaded")

return Test