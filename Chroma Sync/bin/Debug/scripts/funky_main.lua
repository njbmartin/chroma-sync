local Colore = clr.Corale.Colore.Core
local Thread = clr.System.Threading.Thread
local c = Colore.Color.Purple
function play_anim(json)

local Colors = {
      Blue = Colore.Color.Blue,
      Red = Colore.Color.Red
    }

	while true do
		-- Everthing should be white (strobe effect!!!!)
		
		-- set keyboard colour
		for x=0,5 do
			for y=0,21 do
				Colore.Keyboard.Instance[x,y] =  Colore.Color.Black
			end
		end
		
		for x=0,10 do
			Colore.Keyboard.Instance[math.random(0,5), math.random(0,21)] =  Colors.Blue
		end
		
		-- set mousepad colour
		local custom = NewCustom("mousepad",Colore.Color.Black)
		custom.Colors[math.random(0,14)] = Colore.Color.White
		custom.Colors[math.random(0,14)] = Colore.Color.Red
		custom.Colors[math.random(0,14)] = Colore.Color.Blue
		Colore.Mousepad.Instance.SetCustom(custom)
		-- set mouse colour
		local mouseCustom = NewCustom("mouse",Colore.Color.Black)		
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.White
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.Red
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.Blue
		Colore.Mouse.Instance.SetCustom(mouseCustom)
		-- set keypad colour
		local keypadCustom = NewCustom("keypad",Colore.Color.Black)

		keypadCustom[math.random(0,4),math.random(0,5)] = Colore.Color.White
		keypadCustom[math.random(0,4),math.random(0,5)] = Colore.Color.Red
		keypadCustom[math.random(0,4),math.random(0,5)] = Colore.Color.Blue
		-- WASD
		--keypadCustom[1,2] = c
		--keypadCustom[2,1] = c
		--keypadCustom[2,2] = c
		--keypadCustom[2,3] = c
		Colore.Keypad.Instance.SetCustom(keypadCustom)
		-- We don't want to spam the SDK, so throttle to 50ms
		Thread.Sleep(50)
	end
end

--play_anim()

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
			Colore.Mousepad.Instance.SetCustom(custom)
			do return end
		end
		
		if event.e == "WM_LBUTTONDOWN" then
			Colore.Mouse.Instance.SetAll(c)
			Thread.Sleep(100)
			Colore.Mouse.Instance.Clear()
			do return end
		end
		--[[
		
		-- set keyboard colour
			Colore.Keyboard.Instance.SetPosition(math.random(0,5), math.random(1,18), c, true)
		
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
		Colore.Keypad.Instance.SetCustom(keypadCustom)
		-- We don't want to spam the SDK, so throttle to 50ms
		
		]]

end


--RegisterForEvents("MouseEvents", mouse_event)