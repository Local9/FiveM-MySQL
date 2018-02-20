Citizen.CreateThread(function()
	-- this call causes a hitch, loading multiple small files is better
	local data = LoadResourceFile(GetCurrentResourceName(), "sql/MySQLTest.sql");
	print("Starting MySQL-Lua Performance Test")

	local lastcb = os.clock()
	local allTimes = {}
	for i = 1,12 do
		local y = os.clock()
		local n = 0
		for line in data:gmatch("([^\n]*)\n?") do
			if line ~= "" and line ~= nil then
				if n < 2 then
					exports["GHMattiMySQL"]:Query(line)
				else
					exports["GHMattiMySQL"]:QueryAsync(line, {}, function() lastcb = os.clock() end)
				end
				lastcb = os.clock()
			end
			n = n + 1
		end
		Citizen.Wait(5000)
		print(string.format("Finished "..i.." / 12 .sql Executions"))
		table.insert(allTimes, (lastcb - y)*1000)
	end
	local avg = 0
	for _,v in pairs(allTimes) do
		avg = avg + v
		print("Used "..v.."ms to complete that bunch of SQL Commands")
	end
	print("Averaging at "..(avg/12).."ms")
end)

function luaExecTime(t)
	print(string.format("Lua Query execution time: %.0fms\n", (os.clock() - t)*1000))
end