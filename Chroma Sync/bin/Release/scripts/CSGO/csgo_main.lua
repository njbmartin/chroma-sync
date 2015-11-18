-- All functions must sit within a unique class
-- This prevents any conflicts with other lua scripts
CSGO_Example = {}

local Colore = clr.Corale.Colore
local Razer =  Colore.Razer
local Thread = clr.System.Threading.Thread


-- Theme Options

local Theme = {
	Colors = {
		None = Colore.Core.Color(0, 0, 0),
		Dead = Colore.Core.Color(255, 0, 0),
		Freeze = Colore.Core.Color(255, 255, 255),
		Menu = Colore.Core.Color(255, 255, 255),
		Counter =   Colore.Core.Color(0, 0, 50),
		Terrorists = Colore.Core.Color(50, 50, 0),
		Health = {
			Low = Colore.Core.Color(255, 0, 0),
			Full = Colore.Core.Color(0, 255, 0)
		},
		Armor = {
			Low = Colore.Core.Color(60, 60, 60),
			Full = Colore.Core.Color(255, 255, 255)
		},
		
		Ammo = {
			Low = Colore.Core.Color(255, 0, 0),
			Full = Colore.Core.Color(0, 255, 0)
		}
	}
}

-- CS:GO Specific fields

local team = "NA"
local _isAnimating = false
local _phase = "NA"
local _activity = "NA"
local _helmet = false
local _planted = false

function CSGO_Example.SetAll(color)
	
	for x=0,5 do
		for y=0,21 do
			Keyboard[x,y] =  color
		end
	end
end

-- FreezeTime Animation
CSGO_Example.FreezeTime = coroutine.create(function ()
	while true do
		_isAnimating = true
		if _phase ~= "freezetime" then
			CSGO_Example.SetTeam(_team)
			_isAnimating = false
			coroutine.yield()		
		end
		--Keyboard.SetAll(Colore.Core.Color.Pink)
		Keyboard.SetKey(Razer.Keyboard.Key.Escape, Theme.Colors.Freeze)
		Thread.Sleep(200)
		--Keyboard.SetAll(Colore.Core.Color.Blue)
		Keyboard.SetKey(Razer.Keyboard.Key.Escape, Colore.Core.Color.Black)
		Thread.Sleep(200)
	end
end)

CSGO_Example.Planted = coroutine.create(function ()
	while true do
		_isAnimating = true
		if _planted ~= "planted" then
			CSGO_Example.SetTeam(_team)
			_isAnimating = false
			coroutine.yield()		
		end
		--Keyboard.SetAll(Colore.Core.Color.Pink)
		Keyboard.SetKey(Razer.Keyboard.Key.Five, Theme.Colors.Dead)
		Thread.Sleep(100)
		--Keyboard.SetAll(Colore.Core.Color.Blue)
		Keyboard.SetKey(Razer.Keyboard.Key.Five, Colore.Core.Color.Black)
		Thread.Sleep(200)
	end
end)

function CSGO_Example.RoundHandler(round)
	if round["phase"] ~= _phase then
		--DebugLua("phase changed: " .. round["phase"])
		_phase = round["phase"]
	end
	
	if round["bomb"] ~= _planted then
		--DebugLua("phase changed: " .. round["phase"])
		_planted = round["bomb"]
	end
	
	
	if _planted == "planted" then -- Check if Phase is FreezeTime
		--DebugLua("phase is now freezetime")
		coroutine.resume(CSGO_Example.Planted)
	end
	
	if _phase == "freezetime" then -- Check if Phase is FreezeTime
		--DebugLua("phase is now freezetime")
		coroutine.resume(CSGO_Example.FreezeTime)
	end
	
	
end

function CSGO_Example.PlayerHandler(player)
	
	if player["activity"] ~= _activity then
		_activity = player["activity"]
				
		if _activity == "menu" then
			CSGO_Example.SetAll(Theme.Colors.Terrorists)
			Keyboard.SetKey(Razer.Keyboard.Key.C, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.S, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.G, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.O, Theme.Colors.Menu)
		else
			CSGO_Example.SetTeam(player["team"])
		end
	end

	if player["state"] ~= nil then
		if player["state"]["helmet"] ~= _helmet then
			_helmet = player["state"]["helmet"]
			if _helmet then
				CSGO_Example.SetTeam(_team)
			else
				CSGO_Example.SetAll(Colore.Core.Color.Red)
				
			end
		end
		
		if (_activity ~="menu" and _helmet) then
			local health = math.ceil((4 / 100) * ConvertInt(player["state"]["health"]))
			for i=1, 4 do
				if health >= i then
					Keyboard[0,2+i]= Theme.Colors.Health.Full
				else
					Keyboard[0,2+i]= Theme.Colors.Health.Low
				end
			end
			
			local armor = math.ceil((4 / 100) * ConvertInt(player["state"]["armor"]))
			for i=1, 4 do
				if armor >= i then
					Keyboard[0,6+i]= Theme.Colors.Armor.Full
				else
					Keyboard[0,6+i]= Theme.Colors.None
				end
			end
			
			-- WEAPONS
			
			local Set = {
				One = Theme.Colors.None,
				Two = Theme.Colors.None,
				Three = Theme.Colors.None,
				Four = Theme.Colors.None,
				Five = Theme.Colors.None
			}
			
			for i=0,10 do --pseudocode
				local color = Theme.Colors.None
				--Keyboard[1,1+i]= Theme.Colors.None
				local weapon = player["weapons"]["weapon_" .. i]
    			if weapon ~= nil then
					local type= weapon["type"]
					
					if type == "Pistol" then
						color = Theme.Colors.Ammo.Full
						if weapon["state"]== "active" then
							color = Theme.Colors.Menu
						end
						Set.Two = color
					elseif type == "Knife" then
						color = Theme.Colors.Ammo.Full
						if weapon["state"] == "active" then
							color = Theme.Colors.Menu
						end
						Set.Three = color

					elseif type == "Grenade" then
						color = Theme.Colors.Ammo.Full
						if weapon["state"] == "active" then
							color = Theme.Colors.Menu
						end
						Set.Four = color
					elseif type == "C4" then
						color = Theme.Colors.Ammo.Full
						if weapon["state"] == "active" then
							color = Theme.Colors.Menu
						end
						Set.Five = color
					else
						color = Theme.Colors.Ammo.Full
						if weapon["state"] == "active" then
							color = Theme.Colors.Menu
						end
						Set.One = color
					end
					local ammo = weapon["ammo_clip"]
					if (weapon["state"] == "active" and ammo ~= nil) then
						local mouseTotal = math.ceil((7 / ConvertInt(weapon["ammo_clip_max"])) * ConvertInt(ammo))
						local mouseCustom = NewCustom("mouse")
						c = Theme.Colors.Ammo.Full
							
						if (mouseTotal < 3) then
							c = Theme.Colors.Ammo.Low
						end
							
						for i=0, 7 do
								
							if(i >= mouseTotal) then
								c = Theme.Colors.None
							end
							
							mouseCustom.Colors[17 - i] = c
							mouseCustom.Colors[9 - i] = c
							
						end
						Mouse.SetCustom(mouseCustom)
					end
				end
			end
			
			Keyboard.SetKey(Razer.Keyboard.Key.One, Set.One)
			Keyboard.SetKey(Razer.Keyboard.Key.Two, Set.Two)
			Keyboard.SetKey(Razer.Keyboard.Key.Three, Set.Three)
			Keyboard.SetKey(Razer.Keyboard.Key.Four, Set.Four)
			Keyboard.SetKey(Razer.Keyboard.Key.Five, Set.Five)
			
			
			
			
			
		end
	end
end

function CSGO_Example.SetTeam(team)
	_team = team
	if _team == "CT" then
		CSGO_Example.SetAll(Theme.Colors.Counter)
	elseif _team == "T" then
		CSGO_Example.SetAll(Theme.Colors.Terrorists)
	end
	-- WASD
	Keyboard.SetKey(Razer.Keyboard.Key.W, Theme.Colors.Menu)
	Keyboard.SetKey(Razer.Keyboard.Key.A, Theme.Colors.Menu)
	Keyboard.SetKey(Razer.Keyboard.Key.S, Theme.Colors.Menu)
	Keyboard.SetKey(Razer.Keyboard.Key.D, Theme.Colors.Menu)
end

-- our main function to handle the data 
function CSGO_Example.handleData(json)	
	-- Get the current phase (if any)
	
	player = json["player"]
	CSGO_Example.PlayerHandler(player)
	
	round = json["round"]
	if round ~= nil then
		CSGO_Example.RoundHandler(round)
	end
	
	--phase = json["round"]["phase"]
	--CSGO_Example.PhaseHandler(phase)
	
end

-- Finally, we must register this script in order to receive data
RegisterForEvents("Counter-Strike: Global Offensive", CSGO_Example.handleData)