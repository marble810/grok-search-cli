# 测试自查清单

本清单覆盖 grok-search-cli v1 全部功能点，每个 `[ ]` 对应一个测试用例。

---

## 安装与卸载 (9 项)

- [ ] Bash 安装器：默认安装最新版
- [ ] Bash 安装器：指定版本安装
- [ ] Bash 安装器：自定义目录
- [ ] Bash 安装器：重复安装（升级）
- [ ] PowerShell 安装器：默认安装
- [ ] 安装器不收集 API Key
- [ ] Bash 卸载器：默认目录卸载
- [ ] Bash 卸载器：自定义目录与重复执行
- [ ] PowerShell 卸载器：默认目录与边界提示

## Auth Login (6 项)

- [ ] 交互式登录（masked input）
- [ ] 空 API Key 拒绝
- [ ] stdin 登录（--api-key-stdin）
- [ ] stdin 为空时拒绝
- [ ] 有 stdin 但无 --api-key-stdin 时拒绝
- [ ] --api-key-stdin 无 stdin 时拒绝

## Auth Status (5 项)

- [ ] 环境变量优先级最高
- [ ] .env 文件次之
- [ ] .env 向上搜索
- [ ] Managed store 最低优先级
- [ ] 未配置状态输出

## Auth Logout (4 项)

- [ ] 清除 managed store
- [ ] 重复 logout 无副作用
- [ ] Logout 后提示 env 仍生效
- [ ] Logout 后提示 .env 仍生效

## 安全 (3 项)

- [ ] 凭证文件权限 0600 (Unix)
- [ ] Auth status 不泄露 key 值
- [ ] Auth logout 不泄露 key 值

---

## 搜索模式 (3 项)

- [ ] Web 搜索 (--tool web)
- [ ] X 搜索 (--tool x)
- [ ] Both 搜索 (--tool both)

## 查询输入 (6 项)

- [ ] 位置参数输入
- [ ] stdin 输入
- [ ] stdin+位置参数同时提供 → 拒绝
- [ ] 无查询 → 拒绝
- [ ] 空 stdin → 拒绝
- [ ] 多词位置参数

## Web 过滤 (4 项)

- [ ] 单域名白名单
- [ ] 多域名白名单
- [ ] 域名黑名单
- [ ] 白名单+黑名单组合

## X 过滤 (5 项)

- [ ] Handle 白名单
- [ ] Handle 黑名单
- [ ] 日期范围 (from + to)
- [ ] 仅 from-date
- [ ] 仅 to-date

## 错误处理 (4 项)

- [ ] 无效 --tool 值 → error
- [ ] 缺少 --tool → error
- [ ] 未配 API Key → error
- [ ] 无效 API Key → API error

---

## 输出契约 (3 项)

- [ ] JSON 结构完整性 (tool, model, answer, citations, id)
- [ ] stdout 纯净性（无日志）
- [ ] python 可解析

## Help (5 项)

- [ ] 根帮助
- [ ] --help / -h 别名
- [ ] help search
- [ ] help auth
- [ ] 未知 help 主题回退

## Describe (8 项)

- [ ] 基础 describe 输出
- [ ] JSON 顶层结构 (cli_name, commands, etc.)
- [ ] search 命令描述完整
- [ ] auth 命令描述完整
- [ ] discovery_commands 自描述
- [ ] credentials 信息完整
- [ ] describe 不需要凭证
- [ ] describe 不访问网络

## 一致性 (2 项)

- [ ] help 与 describe 的 flag 一致
- [ ] help 与 describe 的凭证源一致

---

**总计**: 67 项

**通过**: \_\_\_ / 67
**失败**: \_\_\_ / 67
**跳过**: \_\_\_ / 67
