using Microsoft.AspNetCore.Mvc;
using TodoManagement.Api.Models;
using TodoManagement.Api.Services;

namespace TodoManagement.Api.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
	private readonly ItemRepository _itemRepository;

	public ItemsController(ItemRepository itemRepository)
	{
		_itemRepository = itemRepository;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
		var items = await _itemRepository.GetAllAsync(cancellationToken);
		return Ok(items);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] TodoItem item, CancellationToken cancellationToken)
	{
		var insertedId = await _itemRepository.CreateAsync(item, cancellationToken);

		return Ok(new
		{
			acknowledged = true,
			insertedId
		});
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(string id, [FromBody] TodoItem item, CancellationToken cancellationToken)
	{
		var result = await _itemRepository.UpdateNameAsync(id, item.Name, cancellationToken);

		return Ok(new
		{
			acknowledged = result.IsAcknowledged,
			matchedCount = result.MatchedCount,
			modifiedCount = result.ModifiedCount
		});
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
	{
		var result = await _itemRepository.DeleteAsync(id, cancellationToken);

		return Ok(new
		{
			acknowledged = result.IsAcknowledged,
			deletedCount = result.DeletedCount
		});
	}
}