local crdreamBlock = {}

crdreamBlock.name = "Tetrahelper/CrossRoomDreamBlock"
crdreamBlock.fillColor = {0.0, 0.0, 0.0}
crdreamBlock.borderColor = {1.0, 1.0, 1.0}
crdreamBlock.nodeLineRenderType = "line"
crdreamBlock.nodeLimits = {0, 1}
crdreamBlock.placements = {
    name = "cross_room_dream_block",
  
    data = {
        fastMoving = false,
        below = false,
        oneUse = false,
        width = 8,
        height = 8
    }
}

function crdreamBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

return crdreamBlock