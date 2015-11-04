Minecraft_Example = {}

local colore = clr.Corale.Colore.Core
local keyboard = colore.Keyboard.Instance
local thread = clr.System.Threading.Thread

function Minecraft_Example.HandleHealth()
    keyboard.SetAll(colore.Color(0,0,255))
end

-- our main function to handle the data 
function Minecraft_Example.handleData(json)	
    DebugLua("Recived data")
    DebugLua(json)

	Minecraft_Example.HandleHealth()
end

-- Finally, we must register this script in order to receive data
RegisterForEvents("Minecraft", Minecraft_Example.handleData)