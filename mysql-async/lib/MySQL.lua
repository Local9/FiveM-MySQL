-- Replacement for mysql-async/lib/MySQL.lua
-- Based on brouznouf's similar file.

MySQL = {
    Async = {},
    Sync = {}
}

function MySQL.Sync.execute(query, parameters)
    return exports["GHMattiMySQL"]:Query(query, parameters)
end

function MySQL.Sync.fetchAll(query, parameters)
    return exports["GHMattiMySQL"]:QueryResult(query, parameters)
end

function MySQL.Sync.fetchScalar(query, parameters)
    return exports["GHMattiMySQL"]:QueryScalar(query, parameters)
end

MySQL.Sync.insert = MySQL.Sync.execute

function MySQL.Async.execute(query, parameters, callback)
    exports["GHMattiMySQL"]:QueryAsync(query, parameters, callback)
end

function MySQL.Async.fetchAll(query, parameters, callback)
    exports["GHMattiMySQL"]:QueryResultAsync(query, parameters, callback)
end

function MySQL.Async.fetchScalar(query, parameters, callback)
    exports["GHMattiMySQL"]:QueryScalarAsync(query, parameters, callback)
end

MySQL.Async.insert = MySQL.Async.execute

local isReady = false
AddEventHandler("GHMattiMySQLStarted", function()
    isReady = true
end)

function MySQL.ready(callback)
    if isReady then
        callback()
        return
    end
    AddEventHandler("GHMattiMySQLStarted", callback)
end
