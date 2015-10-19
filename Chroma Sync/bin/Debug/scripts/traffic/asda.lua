-- Client for the Yahoo Traffic API (http://developer.yahoo.com/traffic/rest/V1/index.html)
-- using JSON and Lua
-- Matt Croydon (matt@ooiio.com) http://postneo.com

http = require("socket.http") -- http://www.cs.princeton.edu/~diego/professional/luasocket/
json = require("json") -- http://json.luaforge.net/

-- Retrieve traffic information for Kansas City, MO
r, c, h = http.request("http://local.yahooapis.com/MapsService/V1/trafficData?appid=LuaDemo&city=Kansas+City&state=MO&output=json")

if c == 200 then
    -- Process the response
    results = json.decode(r)["ResultSet"]["Result"]
    -- Iterate over the results
    for i=1,table.getn(results) do
        print("Result "..i..":")
        table.foreach(results[i], print)
        print()
    end
end