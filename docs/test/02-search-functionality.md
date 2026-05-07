# 02 - 搜索功能测试

> 本节所有搜索测试需要有效的 `XAI_API_KEY` 配置。
> 建议先设置：`export XAI_API_KEY="your-key"`

---

## 2.1 基础搜索模式

### 2.1.1 Web 搜索

**WHEN**: 运行
```bash
grok-search-cli --tool web "latest developments in quantum computing"
```

**THEN**:
- [ ] stdout 输出一个有效的 JSON 文档
- [ ] `tool` 字段值为 `"web"`
- [ ] `model` 字段值为 `"grok-4-1-fast-reasoning"`
- [ ] `answer` 字段为非空字符串，包含搜索结果文本
- [ ] `citations` 为数组，包含至少一个引用对象（含 `url` 字段）
- [ ] 退出码为零
- [ ] stderr 无错误输出

### 2.1.2 X(Twitter) 搜索

**WHEN**: 运行
```bash
grok-search-cli --tool x "AI safety"
```

**THEN**:
- [ ] `tool` 字段值为 `"x"`
- [ ] JSON 结构完整（同 web 搜索格式）
- [ ] `citations` 数组中的引用通常包含来自 X 的链接
- [ ] 退出码为零

### 2.1.3 Both 搜索

**WHEN**: 运行
```bash
grok-search-cli --tool both "Tesla stock analysis"
```

**THEN**:
- [ ] `tool` 字段值为 `"both"`
- [ ] JSON 结构完整
- [ ] 一次调用同时搜索 Web 和 X
- [ ] 退出码为零

---

## 2.2 查询输入方式

### 2.2.1 位置参数输入

**WHEN**: 运行
```bash
grok-search-cli --tool web "climate change impact 2026"
```

**THEN**:
- [ ] 搜索正常执行，使用 `"climate change impact 2026"` 作为查询文本
- [ ] 退出码为零

### 2.2.2 stdin 输入

**WHEN**: 运行
```bash
printf "renewable energy breakthroughs" | grok-search-cli --tool web
```

**THEN**:
- [ ] 搜索正常执行，使用 stdin 提供的文本作为查询
- [ ] JSON 输出正常
- [ ] 退出码为零

### 2.2.3 同时提供位置参数和 stdin（应拒绝）

**WHEN**: 运行
```bash
echo "query from stdin" | grok-search-cli --tool web "positional query"
```

**THEN**:
- [ ] stderr 输出 "error: provide the query as a positional argument OR via stdin, not both"
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.2.4 不提供查询（应拒绝）

**WHEN**: 运行
```bash
grok-search-cli --tool web
```

**THEN**:
- [ ] stderr 输出 "error: provide a search query as a positional argument or via stdin"
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.2.5 stdin 为空（应拒绝）

**WHEN**: 运行
```bash
echo "" | grok-search-cli --tool web
```

**THEN**:
- [ ] stderr 输出 "error: stdin was empty; provide a query string"
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.2.6 多词位置参数

**WHEN**: 运行
```bash
grok-search-cli --tool web the future of AGI in enterprise
```

**THEN**:
- [ ] 所有词按顺序拼接为查询 ("the future of AGI in enterprise")
- [ ] 搜索正常执行

---

## 2.3 Web 搜索域名过滤

### 2.3.1 单域名白名单

**WHEN**: 运行
```bash
grok-search-cli --tool web "GPT-5 release" --allow-domain openai.com
```

**THEN**:
- [ ] 搜索结果中的 citations 应全部来自 `openai.com` 或子域名
- [ ] JSON 输出正常

### 2.3.2 多域名白名单

**WHEN**: 运行
```bash
grok-search-cli --tool web "AI research" --allow-domain arxiv.org --allow-domain nature.com
```

**THEN**:
- [ ] 搜索结果优先包含来自这两个域名的内容
- [ ] JSON 输出正常

### 2.3.3 域名黑名单

**WHEN**: 运行
```bash
grok-search-cli --tool web "smartphone reviews" --exclude-domain reddit.com --exclude-domain quora.com
```

**THEN**:
- [ ] citations 中不应出现 `reddit.com` 和 `quora.com`
- [ ] JSON 输出正常

### 2.3.4 域名白名单+黑名单组合

**WHEN**: 运行
```bash
grok-search-cli --tool web "programming tutorials" --allow-domain github.com --exclude-domain gist.github.com
```

**THEN**:
- [ ] 搜索正常执行
- [ ] 白名单和黑名单同时生效

---

## 2.4 X 搜索过滤

### 2.4.1 Handle 白名单

**WHEN**: 运行
```bash
grok-search-cli --tool x "new model release" --allow-handle OpenAI --allow-handle AnthropicAI
```

**THEN**:
- [ ] 搜索正常执行
- [ ] 结果优先来自指定账号

### 2.4.2 Handle 黑名单

**WHEN**: 运行
```bash
grok-search-cli --tool x "crypto news" --exclude-handle spamaccount123
```

**THEN**:
- [ ] 搜索结果排除指定账号
- [ ] JSON 输出正常

### 2.4.3 日期范围过滤

**WHEN**: 运行
```bash
grok-search-cli --tool x "conference announcement" --from-date 2026-01-01 --to-date 2026-05-07
```

**THEN**:
- [ ] 搜索正常执行
- [ ] 结果限定在指定日期范围内

### 2.4.4 仅 from-date

**WHEN**: 运行
```bash
grok-search-cli --tool x "tech news" --from-date 2026-04-01
```

**THEN**:
- [ ] 搜索正常执行
- [ ] 结果限定在 2026-04-01 之后

### 2.4.5 仅 to-date

**WHEN**: 运行
```bash
grok-search-cli --tool x "WWDC recap" --to-date 2025-12-31
```

**THEN**:
- [ ] 搜索正常执行
- [ ] 结果限定在 2025-12-31 之前

---

## 2.5 错误处理

### 2.5.1 无效 --tool 值

**WHEN**: 运行
```bash
grok-search-cli --tool google "something"
```

**THEN**:
- [ ] stderr 输出 "error: invalid tool 'google': must be web, x, or both"
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.5.2 缺少 --tool 标志

**WHEN**: 运行
```bash
grok-search-cli "some query"
```

**THEN**:
- [ ] stderr 输出 "error: --tool (web|x|both) is required"
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.5.3 未配置 API Key

**前提**:
```bash
unset XAI_API_KEY
grok-search-cli auth logout
cd $(mktemp -d)  # 确保无 .env 文件
```

**WHEN**: 运行
```bash
grok-search-cli --tool web "test query"
```

**THEN**:
- [ ] stderr 输出包含 "error: XAI_API_KEY is not set"
- [ ] stderr 输出包含 `grok-search-cli auth login` 的指引
- [ ] stdout 无输出
- [ ] 退出码为非零（1）

### 2.5.4 无效 API Key（API 返回错误）

**前提**: 设置一个明显无效的 key
```bash
export XAI_API_KEY="xai-invalid-key-12345"
```

**WHEN**: 运行
```bash
grok-search-cli --tool web "test"
```

**THEN**:
- [ ] stderr 输出包含 "error: xAI API returned 401"（或非 200 状态码）
- [ ] stdout 无输出
- [ ] 退出码为非零（2）

**清理**:
```bash
unset XAI_API_KEY
```

---

## 2.6 输出契约验证

### 2.6.1 JSON 结构完整性

**WHEN**: 执行任意成功的搜索

**THEN**:
- [ ] stdout 有且仅有一行有效 JSON
- [ ] JSON 包含 `tool` (string)
- [ ] JSON 包含 `model` (string, `"grok-4-1-fast-reasoning"`)
- [ ] JSON 包含 `answer` (string, 非空)
- [ ] JSON 包含 `citations` (array)
- [ ] `citations` 中每个元素包含 `url` (string)
- [ ] `citations` 中的 `title` 字段存在时值为 string
- [ ] JSON 中 `id` 字段存在时值为 string

### 2.6.2 stdout 纯净性

**WHEN**: 执行任意成功的搜索

**THEN**:
- [ ] stdout 除 JSON 外无其他内容（无日志、无进度提示）
- [ ] 所有诊断信息均在 stderr

### 2.6.3 用 python 验证 JSON 可解析

**WHEN**: 运行
```bash
grok-search-cli --tool web "hello world" | python3 -c "import sys,json; d=json.load(sys.stdin); assert 'tool' in d; assert 'answer' in d; assert 'citations' in d; print('VALID')"
```

**THEN**:
- [ ] 输出 "VALID"
- [ ] 退出码为零
