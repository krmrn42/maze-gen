# maze-gen developer tasks. Run `make` (or `make help`) for the list.
# Requires the .NET SDK (net8.0). Thin wrappers over `dotnet` and tasks/*.sh;
# see CLAUDE.md for details.

SOLUTION    := maze-gen.sln
UNIT_FILTER := TestCategory!=Load & TestCategory!=Integration
CASE        ?= 1
SEED        ?= 12345

.DEFAULT_GOAL := help

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}'

.PHONY: restore
restore: ## Restore NuGet packages + local dotnet tools
	dotnet restore $(SOLUTION)
	dotnet tool restore

.PHONY: build
build: ## Build the solution (Debug)
	dotnet build $(SOLUTION) -c Debug

.PHONY: test
test: ## Run the fast unit tests (what CI runs)
	dotnet test $(SOLUTION) -c Debug --filter "$(UNIT_FILTER)"

.PHONY: test-all
test-all: ## Run every test (unit + integration + load)
	dotnet test $(SOLUTION) -c Debug

.PHONY: test-integration
test-integration: ## Run the integration tests
	dotnet test $(SOLUTION) -c Debug --filter "TestCategory=Integration"

.PHONY: test-load
test-load: ## Run the load/statistical tests
	dotnet test $(SOLUTION) -c Debug --filter "TestCategory=Load"

.PHONY: lint
lint: ## Check formatting/style against .editorconfig (no changes made)
	dotnet format $(SOLUTION) --verify-no-changes

.PHONY: format
format: ## Auto-format the code to .editorconfig
	dotnet format $(SOLUTION)

.PHONY: coverage
coverage: ## Coverage report (coverlet + ReportGenerator) -> build/coverage
	./tasks/coverage.sh

.PHONY: perf
perf: ## Performance trace of the `perfrun` workload -> build/perf
	./tasks/perf.sh

.PHONY: demo
demo: build ## Render one maze with two rooms as ASCII (CASE=1 SEED=12345)
	@dotnet run --project maze-gen -c Debug --no-build -- usecase -c $(CASE) -r $(SEED)

.PHONY: clean
clean: ## Remove build outputs
	./tasks/clean.sh

.PHONY: ci
ci: lint build test demo ## What CI runs: lint + build + unit tests + smoke render
