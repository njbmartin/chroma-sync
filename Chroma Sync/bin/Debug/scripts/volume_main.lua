local colore = clr.Corale.Colore.Core
local Razer = clr.Corale.Colore.Razer
local Thread = clr.System.Threading.Thread

function update_volume()
	while true do
		local cv = clr.AudioVolume.AudioVolume.Volume
   		local volume = clr.AudioVolume.AudioVolume.GetMasterVolume()	   
		
		if (math.abs(cv - volume) > 0.0001) then
			
			local headsetTotal = math.ceil(100 * volume)
			local c = colore.Color.Green
			if (headsetTotal >= 30) then
				c = colore.Color(255, 140, 0)
				--clr.ChromaSync.TrayApplicationContext.BalloonTip("Getting things ready", "Chroma Sync is performing first-time setup.\nThis shouldn't take long...",2000)
				
			end
			if (headsetTotal >= 50) then
				c = colore.Color.Red
				
			end
			Headset.SetAll(c)
			
			local mouseTotal = 6 * volume;
			local mousepadTotal = 14 * volume;
			
			-- set mousepad colour
			
			for i=0, 14 do
				
				c = colore.Color.Green
				if (headsetTotal >= 30) then
					c = colore.Color(255, 140, 0)
					end
				if (headsetTotal >= 50) then
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
				if (headsetTotal >= 30) then
					c = colore.Color(255, 140, 0)
					end
				if (headsetTotal >= 50) then
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

local message = clr.System.Windows.Forms.MessageBox.Show("Would", "title", 4, 32)
if message then
	update_volume()
end