# jdiff

a .NET diff library to compare json

## Usage

```csharp
var diff = left.Diff(right, new JsonDiffOptions
{
    SymbolToString = s => s switch
    {
        DiffSymbol.Added => "+",
        DiffSymbol.Removed => "-",
        DiffSymbol.Modified => "*",
        DiffSymbol.Unchanged => "",
        _ => "~"
    }
});
```

### Left

```json
{
  "id": 123,
  "name": "Original Product",
  "price": 29.99,
  "inStock": true,
  "averageRating": null,
  "averageSize": null,
  "categories": [
    "electronics",
    "gadgets"
  ],
  "attributes": {
    "color": "red",
    "size": "M",
    "warranty": "1 year"
  },
  "mechanisms": {
    "type": "automatic",
    "power": "battery"
  },
  "shipping": {
    "method": "standard",
    "cost": 5.99,
    "estimatedDeliveryDays": 5
  },
  "reviews": [
    {
      "user": "user123",
      "rating": 5,
      "comment": "Excellent!"
    },
    {
      "user": "user456",
      "rating": 4,
      "comment": "Good, but expensive."
    }
  ],
  "map": {
    "latitude": 12.3456,
    "longitude": 78.9012
  }
}
```

### Right

```json
{
  "id": 123,
  "name": "Updated Product",
  "price": 31.99,
  "inStock": false,
  "averageRating": null,
  "averagePrice": null,
  "categories": [
    "electronics",
    "gadgets",
    "newCategory"
  ],
  "attributes": {
    "color": "blue",
    "size": "M",
    "warranty": "2 years"
  },
  "mechanisms": [
    {
      "type": "automatic",
      "power": "battery"
    },
    {
      "type": "manual",
      "power": "manual"
    }
  ],
  "shipping": {
    "method": "express",
    "cost": 9.99,
    "estimatedDeliveryDays": "3"
  },
  "reviews": [
    {
      "user": "user123",
      "rating": 5,
      "comment": "Excellent!"
    },
    {
      "user": "user789",
      "rating": 3,
      "comment": "Average product.",
      "date": "2020-01-01"
    }
  ],
  "score": {
    "rating": 4.5,
    "votes": 2
  }
}
```

### Result

```json
{
  "id": 123,
  "*name": "Original Product",
  "*price": 29.99,
  "*inStock": true,
  "averageRating": null,
  "\u002BaverageSize": null,
  "*categories": [
    "electronics",
    "gadgets",
    "newCategory"
  ],
  "*attributes": {
    "*color": "red",
    "size": "M",
    "*warranty": "1 year"
  },
  "*mechanisms": {
    "type": "automatic",
    "power": "battery"
  },
  "*shipping": {
    "*method": "standard",
    "*cost": 5.99,
    "*estimatedDeliveryDays": 5
  },
  "*reviews": [
    {
      "user": "user123",
      "rating": 5,
      "comment": "Excellent!"
    },
    {
      "*user": "user456",
      "*rating": 4,
      "*comment": "Good, but expensive.",
      "-date": "2020-01-01"
    }
  ],
  "\u002Bmap": {
    "latitude": 12.3456,
    "longitude": 78.9012
  },
  "-averagePrice": null,
  "-score": {
    "rating": 4.5,
    "votes": 2
  }
}
```

