local strawberry = {}

strawberry.name = "Tetrahelper/WhiteBerry"
strawberry.depth = -100
strawberry.nodeLineRenderType = "fan"
strawberry.nodeLimits = {0, -1}
strawberry.fieldInformation = {
    order = {
        fieldType = "integer",
    },
    checkpointID = {
        fieldType = "integer"
    }
}

function strawberry.texture(room, entity)
    
      return "collectables/WhiteBerry/normal00"
           
end

strawberry.placements = {
    {
        name = "White Berry (Tetrahelper)",
        data = {
            checkpointID = -1,
            order = -1
        },
    },
    
}

return strawberry