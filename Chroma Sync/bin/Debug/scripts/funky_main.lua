local Colore = clr.Corale.Colore.Core
local Thread = clr.System.Threading.Thread

function play_anim(json)
	while true do
		-- Everthing should be white (strobe effect!!!!)
		c = Colore.Color.Purple
		-- set keyboard colour
		Colore.Keyboard.Instance.SetPosition(math.random(0,5), math.random(1,18), c, true)
		-- set mousepad colour
		local custom = NewCustom("mousepad")
		custom.Colors[math.random(0,14)] = c
		custom.Colors[math.random(0,14)] = c
		custom.Colors[math.random(0,14)] = c
		Colore.Mousepad.Instance.SetCustom(custom)
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
		Thread.Sleep(50)
	end
end

--play_anim()

function mouse_event(event)
	c = Colore.Color.Purple

		if event.e == "WM_MOUSEMOVE" then
			local screen= clr.System.Windows.Forms.Screen.PrimaryScreen.Bounds
			local posX = (math.ceil((100 / screen.Width) * event.pt.x))
			local posY = (math.ceil((100 / screen.Height) * event.pt.y))
			if posX > 50 then
				
			end
			-- Everthing should be white (strobe effect!!!!)
			
			
			-- set mousepad colour
			local custom = NewCustom("mousepad")
			local mpPosX = 10 - math.ceil((6 / 100) * posX)
			DebugLua(mpPosX)
			custom.Colors[mpPosX] = c
			Colore.Mousepad.Instance.SetCustom(custom)
			do return end
		end
		
		if event.e == "WM_LBUTTONDOWN" then
		DebugLua(event.e)
			Colore.Mouse.Instance.SetAll(c)
			Thread.Sleep(200)
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


RegisterForEvents("MouseEvents", mouse_event)