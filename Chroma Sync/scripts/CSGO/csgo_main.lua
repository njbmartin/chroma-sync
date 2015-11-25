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
		Lite = Colore.Core.Color(50, 50, 50),
		Menu = Colore.Core.Color(255, 255, 255),
		Counter =   Colore.Core.Color(0, 0, 50),
		Terrorists = Colore.Core.Color(255, 69, 0),
		Health = {
			Low = Colore.Core.Color(0, 0, 0),
			Full = Colore.Core.Color(60, 0, 0)
		},
		Weapon = {
			Inactive = Colore.Core.Color(60, 0, 0),
			Active = Colore.Core.Color(0, 60, 0)
		},
		Armor = {
			Low = Colore.Core.Color(0, 0, 0),
			Full = Colore.Core.Color(60, 60, 60)
		},
		
		Ammo = {
			Low = Colore.Core.Color(60, 0, 0),
			Full = Colore.Core.Color(0, 60, 0)
		}
	}
}

local Colors = {
	Background = Colore.Core.Color(255, 69, 0), 
	One = Colore.Core.Color.Red,
	Two = Colore.Core.Color.Red,
	Three = Colore.Core.Color(255,140,0),
}

-- CS:GO Specific fields

local _team = "NA"
local _isAnimating = false
local _phase = "NA"
local _activity = "NA"
local _helmet = false
local _planted = false
local _flashed = false
local _burning = false



function CSGO_Example.SetAll(color)
	--keyboard
	for x=0,5 do
		for y=0,21 do
			Keyboard[x,y] =  color
		end
	end
	
	-- mousepad
	local custom = NewCustom("mousepad",color)
	Mousepad.SetCustom(custom)
	
	-- mouse
	local mouseCustom = NewCustom("mouse",color)		
	Mouse.SetCustom(mouseCustom)
	
	--keypad
	local keypadCustom = NewCustom("keypad",color)
	Keypad.SetCustom(keypadCustom)
	
end


function CSGO_Example.SetNumpad(color)

	for x=0,5 do
		for y=15,21 do
			Keyboard[x,y] =  color
		end
	end
	
end

-- FreezeTime Animation
CSGO_Example.FreezeTime = coroutine.create(function ()
local mousepadNumber = 0
	while true do
		_isAnimating = true
		if _phase ~= "freezetime" then
			CSGO_Example.SetTeam(_team)
			_isAnimating = false
			coroutine.yield()		
		end
		--Keyboard.SetAll(Colore.Core.Color.Pink)
		if mousepadNumber > 7 then
			Keyboard.SetKey(Razer.Keyboard.Key.Escape, Theme.Colors.Freeze)
			CSGO_Example.SetNumpad(Theme.Colors.Freeze)
		else
		Keyboard.SetKey(Razer.Keyboard.Key.Escape, Colore.Core.Color.Black)
		CSGO_Example.SetNumpad(Theme.Colors.None)
		end
		Thread.Sleep(50)
		local custom = NewCustom("mousepad",Theme.Colors.None)
		--[[
		custom.Colors[math.random(0,14)] = Colors.One
		custom.Colors[math.random(0,14)] = Colors.Two
		custom.Colors[math.random(0,14)] = Colors.Three
		]]
		
		custom.Colors[mousepadNumber] = Theme.Colors.Freeze
		mousepadNumber = mousepadNumber + 1
		if mousepadNumber >= 15 then
			mousepadNumber = 0
		end
		Mousepad.SetCustom(custom)
	end
end)

-- Flashed Animation
CSGO_Example.Flashed = coroutine.create(function ()
local mousepadNumber = 0
	while true do
	
		_isAnimating = true
		if _flashed == false then
			CSGO_Example.SetTeam(_team)
			_isAnimating = false
			coroutine.yield()		
		end
		--Keyboard.SetAll(Colore.Core.Color.Pink)
		
		CSGO_Example.SetAll(Theme.Colors.Freeze)
		
	end
end)

CSGO_Example.Burning = coroutine.create(function ()
	while true do
		_isAnimating = true
		if _burning ~= true then
			CSGO_Example.SetTeam(_team)
			_isAnimating = false
			coroutine.yield()
		end
		
		-- set keyboard colour
		
		for x=0,5 do
		for y=15,21 do
			Keyboard[x,y] =  Colors.Background
		end
	end
		
		for x=0,5 do
			Keyboard[math.random(0,6), math.random(15,22)] =  Colors.One
		end
		
		-- set mousepad colour
		local custom = NewCustom("mousepad",Colors.Background)
		custom.Colors[math.random(0,14)] = Colors.One
		
		Mousepad.SetCustom(custom)
		
		-- set mouse colour
		local mouseCustom = NewCustom("mouse",Colors.Background)		
		mouseCustom.Colors[math.random(0,17)] = Colors.One
		Mouse.SetCustom(mouseCustom)
		
		-- set keypad colour
		local keypadCustom = NewCustom("keypad",Colors.Background)
		keypadCustom[math.random(0,4),math.random(0,5)] = Colors.One
		
		-- WASD
		--keypadCustom[1,2] = c
		--keypadCustom[2,1] = c
		--keypadCustom[2,2] = c
		--keypadCustom[2,3] = c
		Keypad.SetCustom(keypadCustom)
		-- We don't want to spam the SDK, so throttle to 50ms
		Thread.Sleep(60)
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
		CSGO_Example.SetNumpad(Theme.Colors.Dead)
		local custom = NewCustom("mousepad",Theme.Colors.Dead)
		Mousepad.SetCustom(custom)
		
		
		Thread.Sleep(500)
		--Keyboard.SetAll(Colore.Core.Color.Blue)
		Keyboard.SetKey(Razer.Keyboard.Key.Five, Colore.Core.Color.Black)
		local custom = NewCustom("mousepad",Colore.Core.Color.Black)
		Mousepad.SetCustom(custom)
		CSGO_Example.SetNumpad(Theme.Colors.None)
		Thread.Sleep(500)
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
				_team = "NA"
			CSGO_Example.SetAll(Theme.Colors.Terrorists)
			Keyboard.SetKey(Razer.Keyboard.Key.C, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.S, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.G, Theme.Colors.Menu)
			Keyboard.SetKey(Razer.Keyboard.Key.O, Theme.Colors.Menu)
			do return end
		
		end
		
	end
	
	if player["state"]["flashed"] > 0 then
		_flashed = true
		DebugLua(player["state"]["flashed"])
		coroutine.resume(CSGO_Example.Flashed)
		do return end
	else
		_flashed = false
	end
	
	if player["state"]["burning"] > 0 then
		_burning = true
		DebugLua(player["state"]["burning"])
		coroutine.resume(CSGO_Example.Burning)
		do return end
	else
		_burning = false
	end

	if player["state"] ~= nil then
		if player["state"]["helmet"] ~= _helmet then
			_helmet = player["state"]["helmet"]
			if _helmet == false then
				CSGO_Example.SetAll(Colore.Core.Color.Red)
			do return end
		end
	end
		
		if _team ~= player["team"] then
			_team = player["team"]
			CSGO_Example.SetTeam(_team)
		end
		
		if (_activity ~="menu" and _helmet) then
			local health = math.ceil((4 / 100) * ConvertInt(player["state"]["health"]))
			for i=1, 4 do
				if health >= i then
					Keyboard[0,6+i]= Theme.Colors.Health.Full
				else
					Keyboard[0,6+i]= Theme.Colors.None
				end
			end
			
			local armor = math.ceil((4 / 100) * ConvertInt(player["state"]["armor"]))
			for i=1, 4 do
				if armor >= i then
					Keyboard[0,10+i]= Theme.Colors.Armor.Full
				else
					Keyboard[0,10+i]= Theme.Colors.None
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
						color = Theme.Colors.Weapon.Inactive
						if weapon["state"]== "active" then
							color = Theme.Colors.Weapon.Active
						end
						Set.Two = color
					elseif type == "Knife" then
						color = Theme.Colors.Weapon.Inactive
						if weapon["state"] == "active" then
							color = Theme.Colors.Weapon.Active
						end
						Set.Three = color

					elseif type == "Grenade" then
						color = Theme.Colors.Weapon.Inactive
						if weapon["state"] == "active" then
							color = Theme.Colors.Weapon.Active
						end
						Set.Four = color
					elseif type == "C4" then
						color = Theme.Colors.Weapon.Inactive
						if weapon["state"] == "active" then
							color = Theme.Colors.Weapon.Active
						end
						Set.Five = color
					else
						color = Theme.Colors.Weapon.Inactive
						if weapon["state"] == "active" then
							color = Theme.Colors.Weapon.Active
						end
						Set.One = color
					end
					
					
					-- Current Ammo
					local ammo = weapon["ammo_clip"]
					if (weapon["state"] == "active" and ammo ~= nil) then
						local keyboardTotal = math.ceil((4 / ConvertInt(weapon["ammo_clip_max"])) * ConvertInt(ammo))
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
						
						c = Theme.Colors.Ammo.Full
							
						if (keyboardTotal < 2) then
							c = Theme.Colors.Ammo.Low
						end
							
						for i=0, 3 do
								
							if(i >= keyboardTotal) then
								c = Theme.Colors.None
							end
							Keyboard[0, 3 + i] = c
						end
						
						
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
		DebugLua("team: " .. _team)
		
	if _activity ~= "menu" then
		if team == "CT" then
			CSGO_Example.SetAll(Theme.Colors.Counter)
		else
			CSGO_Example.SetAll(Theme.Colors.Terrorists)
		end
		-- WASD
		Keyboard.SetKey(Razer.Keyboard.Key.W, Theme.Colors.Menu)
		Keyboard.SetKey(Razer.Keyboard.Key.A, Theme.Colors.Menu)
		Keyboard.SetKey(Razer.Keyboard.Key.S, Theme.Colors.Menu)
		Keyboard.SetKey(Razer.Keyboard.Key.D, Theme.Colors.Menu)
	
	else
		_team = "NA"
	end
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