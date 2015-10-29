local colore = clr.Corale.Colore.Core
local Keyboard = colore.Keyboard
local Headset = colore.Headset
local Mousepad = colore.Mousepad
local Mouse = colore.Mouse
local Razer = clr.Corale.Colore.Razer


function update_volume()
	while true do
		local cv = clr.ChromaSync.AudioVolume.Volume
   		local volume = clr.ChromaSync.AudioVolume.GetMasterVolume()	   
		
		if (math.abs(cv - volume) > 0.0001) then
			
			local headsetTotal = math.ceil(10 * volume)
			local c = colore.Color.Green
			if (headsetTotal >= 4) then
				c = colore.Color(255, 140, 0)
			end
			if (headsetTotal >= 5) then
				c = colore.Color.Red
			end
			Headset.Instance.SetAll(c)
			
			local mouseTotal = 6 * volume;
			local mousepadTotal = 14 * volume;
			
			-- set mousepad colour
			local custom = NewCustom("mousepad")
			for i=0, 14 do
				
				c = colore.Color.Green
				if (i >= 7) then
					c = colore.Color.Orange
					end
				if (i >= 10) then
					c = colore.Color.Red
					end
				
				if i >= mousepadTotal then
					c = colore.Color.Black
				end
				custom.Colors[i] = c
				
			end
			Mousepad.Instance.SetCustom(custom)
			
			-- set mouse colour
			local mouseCustom = NewCustom("mouse")		
			for i=0, 7 do
			
				c = colore.Color.Green
				if (i >= 2) then
					c = colore.Color.Orange
					end
				if (i >= 4) then
					c = colore.Color.Red
					end
					
				if(i >= mouseTotal) then
					c = colore.Color.Black
				end
				
				mouseCustom.Colors[17 - i] = c
				mouseCustom.Colors[9 - i] = c
				
			end
			Mouse.Instance.SetCustom(mouseCustom)
			
		end
	
	end
end
 
update_volume()