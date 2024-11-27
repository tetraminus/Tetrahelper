local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local PowerStation = {}

PowerStation.name = "Tetrahelper/PowerStation"
PowerStation.depth = 0
PowerStation.justification = {0.25, 0.25}

PowerStation.placements = {
        name = "PowerStation (Tetrahelper)",
        data = {
           Flag = ""
        }
    }


function PowerStation.sprite(room, entity)
   
    local sprite = drawableSprite.fromTexture("objects/PowerStation/PS_off_Idle_00", entity)

    

    return sprite
end

return PowerStation