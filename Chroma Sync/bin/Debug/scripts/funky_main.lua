local Colore = clr.Corale.Colore.Core
local Thread = clr.System.Threading.Thread
local c = Colore.Color.Purple

local Colors = {
	Background = Colore.Color(60, 60, 60), 
	One = Colore.Color(255,0,0),
	Two = Colore.Color(255,0,0),
	Three = Colore.Color(255,0,0),
}

local Memory = {
	Keyboard = {
		X = 0,
		Y = 0,
	},
	Color = 0.00,
}

function hsvToRgb(h, s, v, a)
  local r, g, b

  local i = math.floor(h * 6);
  local f = h * 6 - i;
  local p = v * (1 - s);
  local q = v * (1 - f * s);
  local t = v * (1 - (1 - f) * s);

  i = i % 6

  if i == 0 then r, g, b = v, t, p
  elseif i == 1 then r, g, b = q, v, p
  elseif i == 2 then r, g, b = p, v, t
  elseif i == 3 then r, g, b = p, q, v
  elseif i == 4 then r, g, b = t, p, v
  elseif i == 5 then r, g, b = v, p, q
  end

  return r * 255, g * 255, b * 255, a * 255
end

function play_anim(json)
local mousepadNumber = 0
	while true do
	
	r,g,b = hsvToRgb(Memory.Color,1,1,1)
	Memory.Color = Memory.Color + 0.002
	Colors.Background = Colore.Color(r,g,b)
	
		-- set keyboard colour
		Colore.Chroma.Instance.SetAll(Colors.Background)
		
		-- We don't want to spam the SDK, so throttle to 50ms
		Thread.Sleep(50)
	end
end

play_anim()

function mouse_event(event)

		if event.e == "WM_MOUSEMOVE" then
			local screen= clr.System.Windows.Forms.Screen.PrimaryScreen.Bounds
			local posX = (math.ceil((100 / screen.Width) * event.pt.x))
			local posY = (math.ceil((100 / screen.Height) * event.pt.y))
			if posX > 50 then
				
			end
			-- Everthing should be white (strobe effect!!!!)
			
			
			-- set mousepad colour
			local custom = NewCustom("mousepad",Colore.Color.Black)
			local mpPosX = 10 - math.ceil((6 / 100) * posX)
			custom.Colors[mpPosX] = c
			Mousepad.SetCustom(custom)
			do return end
		end
		
		if event.e == "WM_LBUTTONDOWN" then
			Mouse.SetAll(c)
			Thread.Sleep(100)
			Mouse.Clear()
			do return end
		end
		--[[
		
		-- set keyboard colour
			Keyboard.SetPosition(math.random(0,5), math.random(1,18), c, true)
		
		-- set mouse colour
		local mouseCustom = NewCustom("mouse")		
		mouseCustom.Colors[math.random(0,17)] = c
		mouseCustom.Colors[math.random(0,17)] = c
		mouseCustom.Colors[math.random(0,17)] = c
		Colore.Mouse.Instance.SetCustom(mouseCustom)
		-- set keypad colour
		local keypadCustom = NewCustom("keypad")

		keypadCustom[math.random(0,4),math.random(0,5)] = c
		keypadCustom[math.random(0,4),math.random(0,5)] = c
		keypadCustom[math.random(0,4),math.random(0,5)] = c
		-- WASD
		--keypadCustom[1,2] = c
		--keypadCustom[2,1] = c
		--keypadCustom[2,2] = c
		--keypadCustom[2,3] = c
		Keypad.SetCustom(keypadCustom)
		-- We don't want to spam the SDK, so throttle to 50ms
		
		]]

end


--RegisterForEvents("MouseEvents", mouse_event)