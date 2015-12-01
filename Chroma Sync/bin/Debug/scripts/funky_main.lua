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

function randomColor()
	r,g,b = hsvToRgb(math.random(),1,math.random())
	return Colore.Color(r,g,b)
end

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

  return r * 255, g * 255, b * 255
end

function play_anim(json)
local mousepadNumber = 0
	while true do
	
	for x=0,5 do
		for y=0,21 do

			Keyboard[x,y] =  randomColor()
		end
	end
	
	for x=0,8 do
		for y=0,6 do
			Mouse[x,y] =  randomColor()
		end
	end
	
	for x=0,3 do
		for y=0,4 do
			Keypad[x,y] =  randomColor()
		end
	end
	
	for x=0,14 do
			Mousepad[x] =  randomColor()
	end
	
		-- We don't want to spam the SDK, so throttle to 50ms
		Thread.Sleep(50)
	end
end

play_anim()

