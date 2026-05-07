# 人工设计流程

本仓库以 OpenSpec 作为变更设计的唯一事实来源。本文档为人类贡献者提供一个精炼的工作流，让你能把一个想法变成已发布的变更，而不跳过设计契约。

## 何时使用此流程

当以下情况发生时，请走这套流程：

- 新增面向用户的能力
- 修改已有 spec 所描述的行为
- 涉及脚本、文档和测试的跨切面实现变更
- 准备在编码开始前需要被评审的工作

如果只是一次纯实现层面的小修小补，不涉及行为变更，那通常不需要完整的设计周期。这种情况下，保持变更足够小，同时仍然加上最窄的验证即可。

## 1. 框定变更

先命名问题，再命名方案。

需要回答的问题：

- 今天什么东西是坏的、缺失的或不清晰的？
- 谁会受影响：用户、Agent、维护者还是发布流程？
- 这是新能力还是对已有能力的修改？
- 第一轮迭代明确不做什么？

变更名用 kebab-case，描述的是结果而非动作，例如：

```bash
add-uninstall-flow
align-branding-with-grok-search-cli
```

## 2. 创建 OpenSpec 变更

先把变更脚手架搭起来，让工作有一个专属的家。

```bash
openspec new change "<change-name>"
openspec status --change "<change-name>" --json
```

用 `openspec status` 了解当前 schema 要求哪些 artifact，以及它们的解锁顺序。

## 3. 编写设计契约

对于本仓库使用的 `spec-driven` schema，通常的 artifact 流程是：

1. `proposal.md`
2. `design.md`
3. `specs/<capability>/spec.md`
4. `tasks.md`

### Proposal

先写"为什么"。

- 说明当前的缺口或痛点。
- 从行为和受影响表面的角度描述变更。
- 准确列出受影响的能力。

### Design

当变更跨多个文件或职责、引入歧义、或需要预先记录技术权衡决策时，写 `design.md`。

本仓库中好的设计文档应该回答：

- 为什么选这个方案而不是其他显而易见的替代方案？
- 实现将守住哪些边界？
- 故意推迟了哪些后续工作？

### Specs

只有在行为契约发生变化时才更新 specs。

- 新需求用 `ADDED` 标记。
- 完整重写已有需求时用 `MODIFIED`。
- 确保每个场景都是可测试的，有清晰的 `WHEN` 和 `THEN`。

### Tasks

把实现拆成小的、可验证的 checkbox。

- 每个任务足够小，能在一个工作 session 内完成。
- 按依赖关系排列任务顺序。
- 明确包含文档和验证工作。

## 4. 按任务实现，别靠记忆

编码之前，拉取 apply instructions 并阅读上下文文件。

```bash
openspec instructions apply --change "<change-name>" --json
```

然后按顺序处理未勾选的任务。

实现守则：

- 从最具体的锚点开始：文件、脚本、测试或命令
- 做能验证当前假设的最小编辑
- 完成第一次有意义的编辑后立即验证
- 只有当一个切片实现并验证完成后才勾选任务

本仓库中每个变更切片应包含：

- 实现
- 行为或用法变更时的文档更新
- 针对所修改表面的聚焦测试

## 5. 先窄验证，再宽验证

用能证伪当前变更的最廉价验证手段。

典型验证顺序：

1. 运行覆盖已编辑行为的聚焦测试或脚本
2. 运行最相关的 `dotnet test` 命令
3. 只有切片稳定后再运行更广泛的验证

示例：

```bash
dotnet test .\tests\grok-search-cli.Tests\grok-search-cli.Tests.csproj --filter ReleaseSmokeTests
openspec validate add-uninstall-flow
```

当存在可执行的验证命令时，不要把 diff 审查当作替代品。

## 6. 干净地完结变更

所有任务勾选完成后，通过 OpenSpec 确认进度：

```bash
openspec instructions apply --change "<change-name>" --json
openspec status --change "<change-name>"
```

如果变更已完成并通过评审，将其归档：

```bash
openspec archive "<change-name>"
```

归档只有在变更目录、specs、任务和验证全部同步后才能进行。

## 决策检查清单

在宣布设计完成之前，确认以下要点：

- Proposal 说明了问题及范围边界。
- Design 记录了重要的技术决策。
- Spec 变更描述的是可观察行为，而非实现细节。
- Tasks 足够具体，无需重新思考整个变更就能执行。
- 实现同时更新了文档和测试。
- 最终验证在最相关的窄面上证明了变更的正确性。

## 建议的评审节奏

对于较大的变更，建议按以下节奏评审：

1. 审查 `proposal.md`：范围是否合理。
2. 审查 `design.md`：方案是否正确，权衡是否得当。
3. 审查 spec 差异：契约是否正确。
4. 在实现开始前审查任务粒度。
5. 在归档前审查最终 diff 和测试结果。

这套流程确保人工设计过程与本仓库已有的结构保持一致：specs 先行，代码其次，归档前必须验证。
