import { useState } from 'react';
import { Layout, Menu, Button, Avatar, Dropdown, Typography } from 'antd';
import {
    TeamOutlined,
    UserOutlined,
    LogoutOutlined,
    PlusOutlined,
    MenuFoldOutlined,
    MenuUnfoldOutlined,
} from '@ant-design/icons';
import { useNavigate, useLocation, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const { Header, Sider, Content } = Layout;
const { Text } = Typography;

export default function AppLayout() {
    const [collapsed, setCollapsed] = useState(false);
    const { user, logout, isAdmin } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const menuItems = [
        {
            key: '/',
            icon: <TeamOutlined />,
            label: 'Employees',
        },
        ...(isAdmin
            ? [
                {
                    key: '/employees/new',
                    icon: <PlusOutlined />,
                    label: 'Add Employee',
                },
            ]
            : []),
    ];

    const userMenu = {
        items: [
            {
                key: 'profile',
                icon: <UserOutlined />,
                label: `${user?.fullName} (${user?.role})`,
                disabled: true,
            },
            { type: 'divider' },
            {
                key: 'logout',
                icon: <LogoutOutlined />,
                label: 'Logout',
                danger: true,
                onClick: handleLogout,
            },
        ],
    };

    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sider
                collapsible
                collapsed={collapsed}
                onCollapse={setCollapsed}
                breakpoint="lg"
                theme="dark"
                style={{
                    background: 'linear-gradient(180deg, #001529 0%, #002140 100%)',
                }}
            >
                <div
                    style={{
                        height: 64,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        borderBottom: '1px solid rgba(255,255,255,0.1)',
                    }}
                >
                    <Text
                        strong
                        style={{
                            color: '#fff',
                            fontSize: collapsed ? 16 : 18,
                            letterSpacing: 1,
                            whiteSpace: 'nowrap',
                        }}
                    >
                        {collapsed ? 'ER' : '🏢 Employee Registry'}
                    </Text>
                </div>
                <Menu
                    theme="dark"
                    mode="inline"
                    selectedKeys={[location.pathname]}
                    items={menuItems}
                    onClick={({ key }) => navigate(key)}
                    style={{ borderRight: 0, marginTop: 8 }}
                />
            </Sider>
            <Layout>
                <Header
                    style={{
                        background: '#fff',
                        padding: '0 24px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                        position: 'sticky',
                        top: 0,
                        zIndex: 10,
                    }}
                >
                    <Button
                        type="text"
                        icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                        onClick={() => setCollapsed(!collapsed)}
                        style={{ fontSize: 16 }}
                    />
                    <Dropdown menu={userMenu} placement="bottomRight">
                        <div style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8 }}>
                            <Avatar
                                style={{
                                    backgroundColor: isAdmin ? '#1677ff' : '#52c41a',
                                }}
                                icon={<UserOutlined />}
                            />
                            <Text strong style={{ fontSize: 14 }}>
                                {user?.fullName}
                            </Text>
                        </div>
                    </Dropdown>
                </Header>
                <Content
                    style={{
                        margin: 24,
                        padding: 24,
                        background: '#f5f5f5',
                        minHeight: 'calc(100vh - 64px - 48px)',
                    }}
                >
                    <Outlet />
                </Content>
            </Layout>
        </Layout>
    );
}
