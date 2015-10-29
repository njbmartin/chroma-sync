Minecraft_Example = {}

local colore = clr.Corale.Colore.Core
local keyboard = colore.Keyboard.Instance
local thread = clr.System.Threading.Thread

function Minecraft_Example.HandleHealth(json)
    --keyboard.SetAll(colore.Color(255,0,0))
	local healthTotal = math.ceil((12 / ConvertInt(json["player"]["maxhealth"])) * ConvertInt(json["player"]["health"]))
	for i=1, 12 do
				
				c = colore.Color.Red
				if (i >= 5) then
					c = colore.Color.Orange
					end
				if (i >= 9) then
					c = colore.Color.Green
					end
				
				if i > healthTotal then
					c = colore.Color.Black
				end
				keyboard[0,i + 2] = c
				
			end
	
end

-- our main function to handle the data 
function Minecraft_Example.handleData(json)	
    DebugLua(json)
	Minecraft_Example.HandleHealth(json)
end

-- Finally, we must register this script in order to receive data
RegisterForEvents("Minecraft", Minecraft_Example.handleData)