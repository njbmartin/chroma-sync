local colore = clr.Corale.Colore.Core
local Razer = clr.Corale.Colore.Razer
local Thread = clr.System.Threading.Thread

function update_volume()
	while true do
		local cv = clr.ChromaSync.AudioVolume.Volume
   		local volume = clr.ChromaSync.AudioVolume.GetMasterVolume()	   
		
		if (math.abs(cv - volume) > 0.0001) then
			
			local headsetTotal = math.ceil(10 * volume)
			local c = colore.Color.Green
			if (headsetTotal >= 5) then
				c = colore.Color(255, 140, 0)
			end
			if (headsetTotal >= 7) then
				c = colore.Color.Red
			end
			Headset.SetAll(c)
			
			local mouseTotal = 6 * volume;
			local mousepadTotal = 14 * volume;
			
			-- set mousepad colour
			
			for i=0, 14 do
				
				c = colore.Color.Green
				if (i >= 6) then
					c = colore.Color(255, 140, 0)
					end
				if (i >= 9) then
					c = colore.Color.Red
					end
				
				if i >= mousepadTotal then
					c = colore.Color.Black
				end
				Mousepad[i] = c
				
			end
			
			
			-- set mouse colour
			
			for i=0, 7 do
			
				c = colore.Color.Green
				if (i >= 2) then
					c = colore.Color(255, 140, 0)
					end
				if (i >= 4) then
					c = colore.Color.Red
					end
					
				if(i >= mouseTotal) then
					c = colore.Color.Black
				end
				
				Mouse[17 - i] = c
				Mouse[9 - i] = c
				
			end
			
			
		end
	Thread.Sleep(50)
	end
end
 
update_volume()