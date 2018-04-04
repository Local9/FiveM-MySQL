-- Replacement for mysql-async/lib/MySQL.lua
-- Based on brouznouf's similar file.

MySQL = {
    Async = {},
    Sync = {},
	Threaded = {}
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

function MySQL.Sync.transaction(querys, params, callback)
    return exports["GHMattiMySQL"]:Transaction(querys, params)
end

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

function MySQL.Async.transaction(querys, params, callback)
    return exports["GHMattiMySQL"]:TransactionAsync(querys, params, callback)
end

MySQL.Threaded.execute = MySQL.Sync.execute
MySQL.Threaded.fetchAll = MySQL.Sync.fetchAll
MySQL.Threaded.fetchScalar = MySQL.Sync.fetchScalar
MySQL.Threaded.insert = MySQL.Sync.insert

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
