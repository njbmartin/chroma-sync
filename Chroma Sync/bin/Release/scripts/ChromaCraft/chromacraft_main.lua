Minecraft_Example = {}

local colore = clr.Corale.Colore.Core
local keyboard = colore.Keyboard.Instance
local thread = clr.System.Threading.Thread

local background_color = colore.Color.Black
local isRaining = false
local time = 12000

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
            c = background_color
        end
        keyboard[0,i + 2] = c

    end

end
function Minecraft_Example.HandleHotbar(json)
    for i=1, 9 do
        c = colore.Color.Blue
        if (i-1 == ConvertInt(json["player"]["hotbar"]["selected"])) then
            c = colore.Color.White
        end
        keyboard[1,i + 1] = c
    end
end
function Minecraft_Example.HandlePotions(json)
    --    DebugLua(#json["player"]["potioneffects"])

    for i=0, 8 do
        c = background_color
        if #json["player"]["potioneffects"] > 0 then
            if i < #json["player"]["potioneffects"] then
                local effect = json["player"]["potioneffects"][i]
                local colors = effect["color"]

                local red = ConvertInt(colors["r"])
                local green = ConvertInt(colors["g"])
                local blue = ConvertInt(colors["b"])

                -- DebugLua(i .. ": " .. effect["name"] .. "  R: " .. red .. " G: " .. green .. " B: " .. blue)

                c = colore.Color (red,green,blue)

            end
        end

        local x = 0;
        local y = 0;

        if i == 0 then
            x = 0;
            y = 0;
        end
        if i == 1 then
            x = 1;
            y = 0;
        end
        if i == 2 then
            x = 2;
            y = 0;
        end
        if i == 3 then
            x = 0;
            y = 1;
        end
        if i == 4 then
            x = 1;
            y = 1;
        end
        if i == 5 then
            x = 2;
            y = 1;
        end
        if i == 6 then
            x = 0;
            y = 2;
        end
        if i == 7 then
            x = 1;
            y = 2;
        end
        if i == 8 then
            x = 2;
            y = 2;
        end
        keyboard[2+y,18+x] = c
    end
end
function Minecraft_Example.HandleBackground(json)	
    isRaining = json["player"]["raining"]
    time = ConvertInt(json["player"]["time"])
    --keyboard.SetAll(background_color)
    
end
function Minecraft_Example.HandleKeys(json)	
    c = colore.Color.Green
    keyboard[2,3] = c
    keyboard[3,3] = c
    keyboard[3,2] = c
    keyboard[3,4] = c
    keyboard[2,6] = c
    
end
-- our main function to handle the data 
function Minecraft_Example.handleData(json)	
    DebugLua(json)
    
    Minecraft_Example.HandleBackground(json)
    Minecraft_Example.HandleHealth(json)
    Minecraft_Example.HandleHotbar(json)
    Minecraft_Example.HandlePotions(json)
    Minecraft_Example.HandleKeys(json)
end

-- Finally, we must register this script in order to receive data
RegisterForEvents("Minecraft", Minecraft_Example.handleData)