AddEventHandler("onServerResourceStart", function(resourceName)
	if(resourceName == GetCurrentResourceName()) then
		TriggerEvent("GHMattiMySQLStarted")
		TriggerEvent("onMySQLReady")
	end
end)