# 03 - 发现与契约测试

> 本节测试 `help` 和 `describe` 命令。无需 API Key，无需网络连接。

---

## 3.1 Help 命令

### 3.1.1 根帮助

**WHEN**: 运行 `grok-search-cli help`

**THEN**:
- [ ] 输出包含 "grok-search-cli - Search the web and X (Twitter) using xAI"
- [ ] 输出包含 "USAGE" 部分，列出四种用法
- [ ] 输出包含 "SEARCH" 部分，列出所有搜索 flag
- [ ] 输出包含 "AUTH" 部分，列出 `auth login`, `auth status`, `auth logout`
- [ ] 输出包含 "DISCOVERY" 部分，列出 `help` 和 `describe`
- [ ] 输出包含 "CREDENTIALS" 部分
- [ ] 输出包含 "OUTPUT" 部分
- [ ] 退出码为零

### 3.1.2 --help 和 -h 别名

**WHEN**: 分别运行
```bash
grok-search-cli --help
grok-search-cli -h
```

**THEN**:
- [ ] 两者输出与 `grok-search-cli help` 完全一致

### 3.1.3 搜索子命令帮助

**WHEN**: 运行 `grok-search-cli help search`

**THEN**:
- [ ] 输出包含 "grok-search-cli - Execute a search query"
- [ ] 输出包含 "--tool (web|x|both)" 标志说明
- [ ] 输出包含所有搜索过滤 flag（`--allow-domain`, `--exclude-domain`, `--allow-handle`, `--exclude-handle`, `--from-date`, `--to-date`）
- [ ] 输出包含 "QUERY RULES" 部分
- [ ] 输出包含 "EXAMPLES" 部分，至少三个示例
- [ ] 输出不包含 auth 相关内容（如 `auth login`, `auth status`）

### 3.1.4 Auth 子命令帮助

**WHEN**: 运行 `grok-search-cli help auth`

**THEN**:
- [ ] 输出包含 "grok-search-cli auth - Manage API credentials"
- [ ] 输出列出三个子命令：`auth login`, `auth status`, `auth logout`
- [ ] 输出包含 `--api-key-stdin` 说明
- [ ] 输出包含 "CREDENTIAL SOURCES (precedence order)" 部分
- [ ] 优先级顺序为: 1. 环境变量, 2. .env, 3. managed store
- [ ] 输出不包含搜索相关 flag（如 `--tool`, `--allow-domain`）

### 3.1.5 未知帮助主题回退

**WHEN**: 运行
```bash
grok-search-cli help nonexistent
```

**THEN**:
- [ ] 输出根帮助（与 `grok-search-cli help` 一致）
- [ ] 退出码为零

---

## 3.2 Describe 命令

### 3.2.1 基础 describe 输出

**WHEN**: 运行 `grok-search-cli describe`

**THEN**:
- [ ] stdout 输出一个有效的 JSON 文档
- [ ] 退出码为零

### 3.2.2 JSON 顶层结构

**WHEN**: 运行
```bash
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
assert d['cli_name'] == 'grok-search-cli', 'cli_name mismatch'
assert 'documentation' in d, 'documentation missing'
assert 'commands' in d, 'commands missing'
assert len(d['commands']) == 2, f'expected 2 commands, got {len(d[\"commands\"])}'
print('top-level: OK')
"
```

**THEN**:
- [ ] 输出 "top-level: OK"

### 3.2.3 Search 命令描述

**WHEN**: 运行
```bash
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
search = [c for c in d['commands'] if c['name'] == 'search'][0]
assert search['description'] != '', 'search desc empty'
assert search['usage'] == 'grok-search-cli [options] <query>', f'usage: {search[\"usage\"]}'
assert len(search['flags']) == 7, f'expected 7 flags, got {len(search[\"flags\"])}'
assert search['flags'][0]['name'] == '--tool', 'first flag not --tool'
assert search['flags'][0]['required'] == True
assert set(search['flags'][0]['values']) == {'web', 'x', 'both'}
assert search['query_rules'] is not None
assert search['credential_prerequisites'] == ['XAI_API_KEY']
assert search['output_mode'] is not None
assert len(search['examples']) >= 3
print('search command: OK')
"
```

**THEN**:
- [ ] 输出 "search command: OK"

### 3.2.4 Auth 命令描述

**WHEN**: 运行
```bash
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
auth = [c for c in d['commands'] if c['name'] == 'auth'][0]
assert len(auth['subcommands']) == 3
sub_names = [s['name'] for s in auth['subcommands']]
assert 'login' in sub_names
assert 'status' in sub_names
assert 'logout' in sub_names
assert len(auth['examples']) >= 2
print('auth command: OK')
"
```

**THEN**:
- [ ] 输出 "auth command: OK"

### 3.2.5 Discovery 命令自描述

**WHEN**: 运行
```bash
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
dc = d['discovery_commands']
assert dc is not None, 'discovery_commands missing'
names = [c['name'] for c in dc]
assert 'help' in names
assert 'describe' in names
print('discovery_commands: OK')
"
```

**THEN**:
- [ ] 输出 "discovery_commands: OK"

### 3.2.6 Credentials 信息

**WHEN**: 运行
```bash
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
creds = d['credentials']
assert creds is not None, 'credentials missing'
assert len(creds['sources']) == 3
assert any('environment variable' in s for s in creds['sources'])
assert any('.env' in s for s in creds['sources'])
assert any('Managed credential store' in s for s in creds['sources'])
assert creds['note'] is not None
print('credentials: OK')
"
```

**THEN**:
- [ ] 输出 "credentials: OK"

### 3.2.7 Describe 不需要凭证

**前提**: 清除凭证状态
```bash
unset XAI_API_KEY
grok-search-cli auth logout
cd $(mktemp -d)
```

**WHEN**: 运行 `grok-search-cli describe`

**THEN**:
- [ ] 正常输出 JSON（不因缺少凭证而失败）
- [ ] 退出码为零

### 3.2.8 Describe 不访问网络

**前提**: 断开网络或使用抓包观察

**WHEN**: 运行 `grok-search-cli describe`

**THEN**:
- [ ] 命令瞬间完成，不产生任何网络请求
- [ ] 输出即时返回

---

## 3.3 输出契约一致性

### 3.3.1 stdout/stderr 分离

**WHEN**: 运行
```bash
grok-search-cli --tool web "test" 2>/tmp/grok-test-stderr.txt 1>/tmp/grok-test-stdout.txt
```

**THEN**:
- [ ] `/tmp/grok-test-stdout.txt` 包含且仅包含一个 JSON 文档
- [ ] `/tmp/grok-test-stderr.txt` 中无 JSON（仅有错误/诊断信息，成功搜索时为空或最少）

**清理**:
```bash
rm -f /tmp/grok-test-stderr.txt /tmp/grok-test-stdout.txt
```

### 3.3.2 Help 输出到 stdout

**WHEN**: 运行
```bash
grok-search-cli help 2>/dev/null
```

**THEN**:
- [ ] stdout 包含完整帮助文本

### 3.3.3 Describe 输出到 stdout

**WHEN**: 运行
```bash
grok-search-cli describe 2>/dev/null | python3 -m json.tool > /dev/null
```

**THEN**:
- [ ] JSON 解析成功

### 3.3.4 错误输出到 stderr

**WHEN**: 运行
```bash
grok-search-cli --tool invalid "test" 2>&1 1>/dev/null
```

**THEN**:
- [ ] 错误信息在 stderr 中可见
- [ ] stdout 无任何输出

---

## 3.4 Discovery 与 Help 内容一致性

### 3.4.1 Flag 覆盖一致性

**WHEN**: 对比 `help search` 和 `describe` 的 flag 列表

```bash
# 从 help 提取 flag（肉眼检查）
grok-search-cli help search

# 从 describe 提取 flag
grok-search-cli describe | python3 -c "
import sys, json
d = json.load(sys.stdin)
search = [c for c in d['commands'] if c['name'] == 'search'][0]
for f in search['flags']:
    print(f['name'])
"
```

**THEN**:
- [ ] help 和 describe 中的 flag 名称完全一致
- [ ] 7 个 flag: `--tool`, `--allow-domain`, `--exclude-domain`, `--allow-handle`, `--exclude-handle`, `--from-date`, `--to-date`

### 3.4.2 凭证源一致性

**WHEN**: 对比 `help auth` 和 `describe` 的凭证源

**THEN**:
- [ ] 两者都列出相同的三个凭证来源
- [ ] 两者都列出相同的优先级顺序
