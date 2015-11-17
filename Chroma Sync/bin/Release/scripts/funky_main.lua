local Colore = clr.Corale.Colore.Core
local Thread = clr.System.Threading.Thread
local c = Colore.Color.Purple
function play_anim(json)

	while true do
		-- Everthing should be white (strobe effect!!!!)
		
		-- set keyboard colour
		for x=0,5 do
			for y=0,21 do
				Keyboard[x,y] =  Colore.Color.Black
			end
		end
		

			Keyboard[math.random(0,5), math.random(0,21)] =  Colore.Color.Red
			Keyboard[math.random(0,5), math.random(0,21)] =  Colore.Color.White
			Keyboard[math.random(0,5), math.random(0,21)] =  Colore.Color.Blue
		
		-- set mousepad colour
		local custom = NewCustom("mousepad",Colore.Color.Black)
		custom.Colors[math.random(0,14)] = Colore.Color.White
		custom.Colors[math.random(0,14)] = Colore.Color.Red
		custom.Colors[math.random(0,14)] = Colore.Color.Blue
		Mousepad.SetCustom(custom)
		-- set mouse colour
		local mouseCustom = NewCustom("mouse",Colore.Color.Black)		
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.White
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.Red
		mouseCustom.Colors[math.random(0,17)] = Colore.Color.Blue
		Mouse.SetCustom(mouseCustom)
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
		Keypad.SetCustom(keypadCustom)
		-- We don't want to spam the SDK, so throttle to 50ms
		Thread.Sleep(50)
	end
end


-- To play this animation, uncomment "play_anim()" (remove "--")

--play_anim()