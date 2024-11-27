local utils = require("utils")
local DashMatchWall = {}

DashMatchWall.name = "TetraHelper/DashMatchWall"
DashMatchWall.color = {0.33, 0.33, 0.33, 0.8}
DashMatchWall.canResize = {true, true}
DashMatchWall.fillColor = {0.0, 0.0, 0.0}
DashMatchWall.borderColor = {1.0, 1.0, 1.0}
DashMatchWall.nodeLineRenderType = "line"
DashMatchWall.placements = {
    name = "Dash Match Wall (Tetrahelper)",
    data = {
        width = 8,
        height = 8,
		dashes = 1,
		GT = false,
        over = false,
    }
}

function DashMatchWall.fillColor(room, entity)
    if entity.dashes == 0 then
        return {68/255, 183/255, 255/255}
    elseif entity.dashes == 1 then
        return {172/255, 50/255, 50/255}
    else
        return {255/255, 109/255, 239/255}
    end
end

function DashMatchWall.borderColor(room, entity)
    -- return darkened versoion of fill color
    local color = DashMatchWall.fillColor(room, entity)
    return {color[1] * 0.5, color[2] * 0.5, color[3] * 0.5}
end


function DashMatchWall.depth(room, entity)
    return entity.over and -12000 or 0
end




return DashMatchWall
