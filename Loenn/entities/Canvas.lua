local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local Canvas = {}

Canvas.name = "Tetrahelper/Canvas"
Canvas.depth = 0
Canvas.justification = {0.25, 0.25}

Canvas.placements = {
    name = "Canvas (Tetrahelper)",
}


function Canvas.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/door/moonDoor11", entity)

   

    return sprite
end

return Canvas